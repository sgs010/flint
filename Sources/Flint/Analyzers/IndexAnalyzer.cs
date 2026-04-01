using System.Text;
using Flint.Common;
using Flint.Vm;
using Mono.Cecil;

namespace Flint.Analyzers
{
	internal class IndexAnalyzer
	{
		#region Properties
		public const int Code = 7;
		#endregion

		#region Interface
		public static void Run(IAnalyzerContext ctx, AssemblyInfo asm, string className = null, string methodName = null)
		{
			var queries = QueryAnalyzer.Analyze(asm, className, methodName);
			foreach (var query in queries)
			{
				var entityMap = new Dictionary<TypeReference, HashSet<MethodReference>>(TypeReferenceEqualityComparer.Instance);

				// collect filter expressions
				foreach (var root in query.Roots)
				{
					foreach (var filter in root.OfCall(asm.EFCoreFilters))
					{
						foreach (var expr in filter.OfCall(asm.LinqExpressions))
						{
							foreach (var mtd in expr.OfMethodof(asm.EntityGetSetMethods))
							{
								var entityType = mtd.Method.DeclaringType;
								if (asm.EntityTypes.Contains(entityType) == false)
									continue; // this is not an entity

								var propertyType = mtd.Method.ReturnType;
								if (propertyType.IsGenericCollection(out var _, asm.EntityTypes))
								{
									// nested entity collection
								}
								else if (asm.EntityTypes.Contains(propertyType))
								{
									// nested entity
								}
								else
								{
									// simple property (int, string and so on)
									if (entityMap.TryGetValue(entityType, out var propertyMap) == false)
									{
										propertyMap = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance);
										entityMap.Add(entityType, propertyMap);
									}
									propertyMap.Add(mtd.Method);
								}
							}
						}
					}
				}

				// report missing indexes
				foreach (var x in entityMap)
				{
					var entityType = asm.EntityTypes.First(y => Are.Equal(x.Key, y));
					var properties = CollectAccessedProperties(asm, entityType, x.Value);

					if (IndexExists(asm, entityType, properties))
						continue;

					var sb = new StringBuilder();
					sb.Append("consider adding index (");
					PrettyPrintIndexColumns(sb, entityType, properties);
					sb.Append(") on entity ").Append(entityType.FullName).Append(" for the query");
					ctx.AddResult(Code, sb.ToString(), query.Method, query.CilPoint);
				}
			}
		}
		#endregion

		#region Implementation
		private static HashSet<PropertyReference> CollectAccessedProperties(AssemblyInfo asm, TypeReference entityType, HashSet<MethodReference> propGet)
		{
			var entityDefinition = asm.EntityTypes.First(x => Are.Equal(x, entityType));
			var index = new HashSet<PropertyReference>(PropertyReferenceEqualityComparer.Instance);
			foreach (var prop in entityDefinition.Properties)
			{
				if (propGet.Contains(prop.GetMethod))
					index.Add(prop);
			}
			return index;
		}

		private static bool IndexExists(AssemblyInfo asm, TypeDefinition entityType, HashSet<PropertyReference> properties)
		{
			if (asm.EntityIndexes.TryGetValue(entityType, out var indexList) == false)
				return false;

			foreach (var index in indexList)
			{
				if (properties.IsSubsetOf(index))
					return true; // index found
			}
			return false;
		}

		private static void PrettyPrintIndexColumns(StringBuilder sb, TypeDefinition entityType, HashSet<PropertyReference> properties)
		{
			// we iterate through all properties to keep the order, because in a set order is not defined

			var needSeparator = false;
			foreach (var prop in entityType.Properties)
			{
				if (properties.Contains(prop) == false)
					continue;
				if (needSeparator)
					sb.Append(',');
				sb.Append(prop.Name);
				needSeparator = true;
			}
		}
		#endregion
	}
}
