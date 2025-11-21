using System.Collections.Frozen;
using System.Collections.Immutable;
using Flint.Common;
using Flint.Vm;
using Mono.Cecil;
using Cil = Flint.Vm.Cil;

namespace Flint.Analyzers
{
	#region QueryInfo
	class QueryInfo
	{
		public required MethodDefinition Method { get; init; }
		public required CilPoint CilPoint { get; init; }
		public required ImmutableArray<Cil.Call> Roots { get; init; }
		public required EntityInfo Entity { get; init; }
	}
	#endregion

	#region EntityInfo
	class EntityInfo
	{
		public required TypeDefinition Type { get; init; }
		public required ImmutableArray<PropertyInfo> Properties { get; init; }
	}
	#endregion

	#region PropertyInfo
	class PropertyInfo
	{
		public required PropertyDefinition Property { get; init; }
		public required EntityInfo Entity { get; init; }
		public required bool Read { get; init; }
		public required bool Write { get; init; }
	}
	#endregion

	#region EntityAnalyzer
	internal class QueryAnalyzer
	{
		#region Interface
		public static bool SomePropertiesAreChanged(EntityInfo entity)
		{
			foreach (var prop in entity.Properties)
			{
				if (prop.Write)
					return true;
				if (prop.Entity != null && SomePropertiesAreChanged(prop.Entity))
					return true;
			}
			return false;
		}

		public static ImmutableArray<QueryInfo> Analyze(AssemblyInfo asm, string className = null, string methodName = null)
		{
			var queries = new List<QueryInfo>();
			foreach (var method in MethodAnalyzer.GetMethods(asm, className, methodName))
			{
				if (AssemblyAnalyzer.MethodHasEFCoreRoots(asm, method))
					Analyze(asm, method, queries);
			}
			return [.. queries];
		}
		#endregion

		#region Implementation
		class XEntity
		{
			public required TypeDefinition Type { get; init; }
			public Dictionary<MetadataToken, XProperty> Properties { get; } = [];
		}

		class XProperty
		{
			public required PropertyDefinition Property { get; init; }
			public XEntity Entity { get; set; }
			public bool Read { get; set; }
			public bool Write { get; set; }
		}

		private static EntityInfo CreateEntityInfo(XEntity entity)
		{
			var propertyOrderMap = entity.Type.Properties
				.Select((prop, i) => (token: prop.MetadataToken, order: i))
				.ToFrozenDictionary(x => x.token, x => x.order);

			return new EntityInfo
			{
				Type = entity.Type,
				Properties = entity.Properties.Values
					.OrderBy(x => propertyOrderMap[x.Property.MetadataToken])
					.Select(CreatePropertyInfo)
					.ToImmutableArray()
			};
		}

		private static PropertyInfo CreatePropertyInfo(XProperty prop)
		{
			return new PropertyInfo
			{
				Property = prop.Property,
				Entity = prop.Entity != null ? CreateEntityInfo(prop.Entity) : null,
				Read = prop.Read,
				Write = prop.Write
			};
		}

		private static IEnumerable<Ast> GetLambdas(Ast expression)
		{
			foreach (var ftn in expression.OfFtn())
			{
				var lambdaMethod = ftn.Method.Resolve().UnwrapAsyncMethod();
				var lambdaExpressions = CilMachine.Eval(lambdaMethod);
				foreach (var expr in lambdaExpressions)
					yield return expr;
			}
		}

		private static IEnumerable<(Ast branch, Cil.Call root, TypeDefinition et)> GetRoots(AssemblyInfo asm, MethodDefinition method)
		{
			foreach (var branch in MethodAnalyzer.Eval(asm, method))
			{
				foreach (var root in branch.OfCall(asm.EFCoreRoots))
				{
					var t = ((GenericInstanceMethod)root.Method).GenericArguments.First();
					if (t is not TypeDefinition et)
						continue;

					// et is T from METHOD<T> (i.e. ToListAsync<T>)
					if (asm.EntityTypes.Contains(et))
						yield return (branch, root, et);

					// some methods (i.e. ToDictionaryAsync) use lambdas, analyze them too
					foreach (var lambda in GetLambdas(branch))
						yield return (lambda, root, et);
				}
			}
		}

		private static void Analyze(AssemblyInfo asm, MethodDefinition method, List<QueryInfo> queryCollection)
		{
			// GetRoots returns all branches where any EFCore root is present.
			// Group branches by CIL point to get queries, because same root can be called in different branches.
			var queries = GetRoots(asm, method).GroupBy(x => x.root.CilPoint).ToList();
			foreach (var query in queries)
			{
				// collect all get/set methods in a query
				var propMap = query
					.SelectMany(x => x.branch.OfCall(asm.EntityGetSetMethods))
					.GroupBy(x => x.Method, MethodReferenceEqualityComparer.Instance)
					.ToDictionary(x => x.Key, x => x.ToList(), MethodReferenceEqualityComparer.Instance);

				var entity = new XEntity { Type = query.First().et };
				foreach (var (branch, _, _) in query)
					FillEntity(asm, branch, propMap, entity);

				queryCollection.Add(new QueryInfo
				{
					Method = method,
					CilPoint = query.Key,
					Roots = query.Select(x => x.root).ToImmutableArray(),
					Entity = CreateEntityInfo(entity)
				});
			}
		}

		private static void FillEntity(AssemblyInfo asm, Ast branch, Dictionary<MethodReference, List<Cil.Call>> propMap, XEntity entity)
		{
			foreach (var prop in entity.Type.Properties)
			{
				var propGet = propMap.GetValueOrDefault(prop.GetMethod);
				var propSet = propMap.GetValueOrDefault(prop.SetMethod);
				if (propGet == null && propSet == null)
					continue; // property is not accessed

				if (entity.Properties.TryGetValue(prop.MetadataToken, out var xprop) == false)
				{
					xprop = new XProperty { Property = prop };
					entity.Properties.Add(prop.MetadataToken, xprop);
				}
				if (propGet != null)
					xprop.Read = true;
				if (propSet != null)
					xprop.Write = true;

				if (prop.PropertyType.IsGenericCollection(out var itemType, asm.EntityTypes))
				{
					// property is entity collection
					if (xprop.Entity == null)
						xprop.Entity = new XEntity { Type = itemType };
					FillEntity(asm, branch, propMap, xprop.Entity);
					xprop.Write |= IsCollectionChanged(branch, propGet);
				}
				else if (asm.EntityTypes.Contains(prop.PropertyType))
				{
					// property is nested entity
					if (xprop.Entity == null)
						xprop.Entity = new XEntity { Type = prop.PropertyType.Resolve() };
					FillEntity(asm, branch, propMap, xprop.Entity);
				}
			}
		}

		private static bool IsCollectionChanged(Ast branch, IReadOnlyCollection<Ast> propGet)
		{
			// check for patterns like prop_GetUsers().Add() and so on
			var addRemoveCalls = branch.OfCallByName(["Add", "Remove"]).ToList();
			foreach (var prop in propGet)
			{
				if (addRemoveCalls.Any(x => x.Instance.Equals(prop)))
					return true;
			}
			return false;
		}
		#endregion
	}
	#endregion
}
