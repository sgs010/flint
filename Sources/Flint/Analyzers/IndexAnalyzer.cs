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
				var filters = new Dictionary<TypeReference, HashSet<MethodReference>>(TypeReferenceEqualityComparer.Instance);

				// collect filter expressions
				foreach (var root in query.Roots)
					foreach (var filter in root.OfCall(asm.LinqFilters))
						foreach (var expr in filter.OfCall(asm.LinqExpressions))
							foreach (var mtd in expr.OfMethodof(asm.EntityGetSetMethods))
							{
								var t = mtd.Method.DeclaringType;
								if (filters.TryGetValue(t, out var props) == false)
								{
									props = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance);
									filters.Add(t, props);
								}
								props.Add(mtd.Method);
							}

				// report issues
				foreach (var x in filters)
				{
					var sb = new StringBuilder();
					sb.Append("consider adding index (");
					PrettyPrintIndexColumns(sb, asm, x.Key, x.Value);
					sb.Append(") on table ");
					PrettyPrintTableName(sb, x.Key);
					sb.Append(" for the query");
					ctx.AddResult(Code, sb.ToString(), query.Method, query.CilPoint);
				}
			}
		}
		#endregion

		#region Implementation
		private static void PrettyPrintTableName(StringBuilder sb, TypeReference t)
		{
			sb.Append(t.Name);
		}

		private static void PrettyPrintIndexColumns(StringBuilder sb, AssemblyInfo asm, TypeReference t, HashSet<MethodReference> props)
		{
			var td = asm.EntityTypes.First(x => Are.Equal(t, x));
			var needSeparator = false;
			foreach (var p in td.Properties)
			{
				if (props.Contains(p.GetMethod) == false)
					continue; // property is not accessed
				if (asm.EntityTypes.Contains(p.PropertyType))
					continue; // property is an entity
				if (needSeparator)
					sb.Append(',');
				sb.Append(p.Name);
				needSeparator = true;
			}
		}
		#endregion
	}
}
