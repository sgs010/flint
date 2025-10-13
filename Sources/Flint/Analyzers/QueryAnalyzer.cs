using System.Collections.Frozen;
using System.Collections.Immutable;
using Flint.Common;
using Flint.Vm;
using Mono.Cecil;
using Cil = Flint.Vm.Cil;
using Match = Flint.Vm.Match;

namespace Flint.Analyzers
{
	#region QueryInfo
	class QueryInfo
	{
		public required MethodDefinition Method { get; init; }
		public required CilPoint CilPoint { get; init; }
		public required ImmutableArray<Ast> Roots { get; init; }
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
			var entities = new List<XEntity>();
			foreach (var method in MethodAnalyzer.GetMethods(asm, className, methodName))
			{
				Analyze(asm, method, entities);
			}

			var queries = new List<QueryInfo>();
			foreach (var grp in entities.GroupBy(x => x.Root.CilPoint))
			{
				var entity = grp.First();
				var roots = grp.Select(x => x.Root).ToImmutableArray();
				var acc = Merge(grp);
				queries.Add(new QueryInfo
				{
					Method = entity.Method,
					CilPoint = grp.Key,
					Roots = roots,
					Entity = CreateEntityInfo(acc)
				});
			}
			return [.. queries];
		}
		#endregion

		#region Implementation
		class XEntity
		{
			public required MethodDefinition Method { get; init; }
			public required Ast Root { get; init; }
			public required TypeDefinition Type { get; init; }
			public required Dictionary<MetadataToken, XProperty> Properties { get; init; }
		}

		class XProperty
		{
			public required PropertyDefinition Property { get; init; }
			public required XEntity Entity { get; init; }
			public required bool Read { get; init; }
			public required bool Write { get; init; }
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

		private static XEntity Clone(XEntity obj)
		{
			if (obj == null)
				return null;

			return new XEntity
			{
				Method = obj.Method,
				Root = obj.Root,
				Type = obj.Type,
				Properties = obj.Properties.ToDictionary(x => x.Key, x => Clone(x.Value))
			};
		}

		private static XProperty Clone(XProperty obj)
		{
			if (obj == null)
				return null;

			return new XProperty
			{
				Property = obj.Property,
				Entity = Clone(obj.Entity),
				Read = obj.Read,
				Write = obj.Write
			};
		}

		private static XEntity Merge(IEnumerable<XEntity> entities)
		{
			XEntity result = null;
			foreach (var entity in entities)
			{
				if (result == null)
					result = Clone(entity);
				else
					result = Merge(result, entity);
			}
			return result;
		}

		private static XEntity Merge(XEntity a, XEntity b)
		{
			if (a.Type.MetadataToken != b.Type.MetadataToken)
				return null;

			var props = new Dictionary<MetadataToken, XProperty>();
			foreach (var token in a.Properties.Keys.Union(b.Properties.Keys))
			{
				var pa = a.Properties.GetValueOrDefault(token);
				var pb = b.Properties.GetValueOrDefault(token);
				XProperty px = null;
				if (pa != null && pb != null)
					px = Merge(pa, pb);
				else if (pa != null && pb == null)
					px = Clone(pa);
				else if (pa == null && pb != null)
					px = Clone(pb);
				if (px != null)
					props.Add(token, px);
			}
			return new XEntity { Method = null, Root = null, Type = a.Type, Properties = props };
		}

		private static XProperty Merge(XProperty a, XProperty b)
		{
			if (a.Property.MetadataToken != b.Property.MetadataToken)
				return null;

			XEntity entity = null;
			if (a.Entity != null && b.Entity != null)
				entity = Merge(a.Entity, b.Entity);
			else if (a.Entity != null && b.Entity == null)
				entity = Clone(a.Entity);
			else if (a.Entity == null && b.Entity != null)
				entity = Clone(b.Entity);

			return new XProperty
			{
				Property = a.Property,
				Entity = entity,
				Read = a.Read | b.Read,
				Write = a.Write | b.Write
			};
		}

		private static void Analyze(AssemblyInfo asm, MethodDefinition method, List<XEntity> entities)
		{
			var expressions = MethodAnalyzer.Eval(asm, method);

			// find roots (methods where IQueryable monad is unwrapped; ToListAsync and so on)
			// for every found root mark every ast accessible from it
			var roots = new HashSet<Cil.Call>();
			var marks = new Dictionary<Ast, List<Ast>>();
			foreach (var expr in expressions)
			{
				var (root, ok) = CaptureAnyRoot(expr,
				[
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsAsyncEnumerable",
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToArrayAsync",
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToDictionaryAsync",
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToHashSetAsync",
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync",
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstAsync",
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync",
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.LastAsync",
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.LastOrDefaultAsync",
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleAsync",
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleOrDefaultAsync",
				]);
				if (ok == false)
					continue;

				roots.Add(root);
				var rootExpressions = marks.GetOrAddValue(root);
				Mark(expr, root, rootExpressions);

				// some methods (i.e. ToDictionaryAsync) use lambdas, analyze them too
				var lambdas = new List<Ast>();
				MethodAnalyzer.CollectLambdaExpressions(rootExpressions, lambdas);
				rootExpressions.AddRange(lambdas);
			}

			// gather accessed properties
			foreach (var root in roots)
			{
				if (marks.TryGetValue(root, out var rootExpressions) == false)
					continue;

				// et is T from METHOD<T> (i.e. ToListAsync<T>)
				var et = (TypeDefinition)((GenericInstanceMethod)root.Method).GenericArguments.First();
				if (asm.EntityTypes.Contains(et) == false)
					continue;

				var entity = CreateEntity(asm, method, root, et, rootExpressions, asm.EntityTypes);
				entities.Add(entity);
			}
		}

		private static (Cil.Call root, bool ok) CaptureAnyRoot(Ast expression, IEnumerable<string> methodNames)
		{
			foreach (var name in methodNames)
			{
				var (captures, ok) = expression.Match(
					new Match.Call(null, name, Match.Any.Args),
					true);
				if (ok == false)
					continue;

				var root = (Cil.Call)captures.Values.First();
				return (root, true);
			}
			return (null, false);
		}

		private static void Mark(Ast expression, Ast root, List<Ast> marks)
		{
			// traverse expression tree top down and mark every node untill we reach root node

			if (expression == null)
				return;
			if (expression == root)
				return;

			marks.Add(expression);
			foreach (var child in expression.GetChildren())
				Mark(child, root, marks);
		}

		private static XEntity CreateEntity(AssemblyInfo asm, MethodDefinition mtd, Ast root, TypeDefinition type, IReadOnlyCollection<Ast> expressions, ISet<TypeDefinition> entityTypes)
		{
			var entityProperties = new Dictionary<MetadataToken, XProperty>(type.Properties.Count);
			foreach (var prop in type.Properties)
			{
				bool propRead = false, propWrite = false;
				XEntity propEnt = null;
				foreach (var expr in expressions)
				{
					// check read (call of get_Property method)
					var propGet = AssemblyAnalyzer.GetMethodFullName(asm, prop.GetMethod);
					var propGetCalls = expr.OfCall(propGet).ToList();
					if (propGetCalls.Count > 0)
						propRead = true;

					// check write (call of set_Property method)
					var propSet = AssemblyAnalyzer.GetMethodFullName(asm, prop.SetMethod);
					if (expr.OfCall(propSet).Any())
						propWrite = true;

					if (propRead == false && propWrite == false)
						continue;

					if (propEnt == null)
					{
						if (prop.PropertyType.IsGenericCollection(out var itemType, entityTypes))
						{
							propEnt = CreateEntity(asm, mtd, root, itemType.Resolve(), expressions, entityTypes);
							propWrite = IsCollectionChanged(expr, propGetCalls);
						}
						else if (entityTypes.Contains(prop.PropertyType))
						{
							propEnt = CreateEntity(asm, mtd, root, prop.PropertyType.Resolve(), expressions, entityTypes);
						}
					}
				}
				if (propRead || propWrite)
					entityProperties.Add(prop.MetadataToken, new XProperty { Property = prop, Entity = propEnt, Read = propRead, Write = propWrite });
			}
			return new XEntity { Method = mtd, Root = root, Type = type, Properties = entityProperties };
		}

		private static bool IsCollectionChanged(Ast expr, IReadOnlyCollection<Ast> captures)
		{
			string[] methods = ["Add", "Remove"];
			foreach (var mtd in methods)
			{
				foreach (var capture in captures)
				{
					var (_, ok) = expr.Match(
						new Match.Call(capture, mtd, Match.Any.Args),
						true);
					if (ok)
						return true;
				}
			}
			return false;
		}
		#endregion
	}
	#endregion
}
