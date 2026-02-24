using System.ComponentModel;
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

				// report issues
				foreach (var x in entityMap)
				{
					var sb = new StringBuilder();
					sb.Append("consider adding index (");
					PrettyPrintIndexColumns(sb, asm, x.Key, x.Value);
					sb.Append(") on entity ").Append(x.Key.FullName).Append(" for the query");
					ctx.AddResult(Code, sb.ToString(), query.Method, query.CilPoint);
				}
			}
		}
		#endregion

		#region Implementation
		private static void PrettyPrintIndexColumns(StringBuilder sb, AssemblyInfo asm, TypeReference t, HashSet<MethodReference> propGet)
		{
			var td = asm.EntityTypes.First(x => Are.Equal(x, t));
			var needSeparator = false;
			foreach (var p in td.Properties)
			{
				if (propGet.Contains(p.GetMethod) == false)
					continue; // property is not accessed
				if (needSeparator)
					sb.Append(',');
				sb.Append(p.Name);
				needSeparator = true;
			}
		}
		#endregion
	}
}
