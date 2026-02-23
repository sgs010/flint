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
				var filters = new Dictionary<TypeReference, XEntity>(TypeReferenceEqualityComparer.Instance);

				// collect filter expressions
				foreach (var root in query.Roots)
				{
					foreach (var filter in root.OfCall(asm.EFCoreFilters))
					{
						if (TryResolveEntity(asm, filter, out var entityType, out var tableName) == false)
							continue;

						foreach (var expr in filter.OfCall(asm.LinqExpressions))
						{
							foreach (var mtd in expr.OfMethodof(asm.EntityGetSetMethods))
							{
								var t = mtd.Method.DeclaringType;
								if (filters.TryGetValue(t, out var entity) == false)
								{
									entity = new XEntity(tableName, new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance));
									filters.Add(t, entity);
								}
								entity.PropGet.Add(mtd.Method);
							}
						}
					}
				}

				// report issues
				foreach (var x in filters)
				{
					var sb = new StringBuilder();
					sb.Append("consider adding index (");
					PrettyPrintIndexColumns(sb, asm, x.Key, x.Value.PropGet);
					sb.Append(") on table ").Append(x.Value.Name).Append(" for the query");
					ctx.AddResult(Code, sb.ToString(), query.Method, query.CilPoint);
				}
			}
		}
		#endregion

		#region Implementation
		record XEntity(string Name, HashSet<MethodReference> PropGet);

		private static bool TryResolveEntity(AssemblyInfo asm, Vm.Cil.Call filter, out TypeReference entityType, out string tableName)
		{
			entityType = null;
			tableName = null;

			// filter is a linq filter method like Where<T>(...)
			// extract it's T
			var filterEntityType = ((GenericInstanceMethod)filter.Method).GenericArguments.First();

			// we need to define which dbset property is references in the filter
			// i.e. db.Users.Where(...) - property is Users and so on
			// so we look for a call of get_PROP method in the filter ast
			foreach (var getProp in filter.OfCall(asm.DbGetMethods))
			{
				if (getProp.Method.ReturnType.IsDbSet(out var dbSetEntityType) == false)
					continue; // this is not a DbSet<>
				if (Are.Equal(filterEntityType, dbSetEntityType) == false)
					continue; // this is not a dbset property we are looking for

				// resolve a dbset property by given get_PROP method
				var dbSetProp = asm.EntityCollections.FirstOrDefault(x => Are.Equal(x.GetMethod, getProp.Method));
				if (dbSetProp == null)
					continue;

				entityType = dbSetEntityType;
				tableName = dbSetProp.Name;
				return true;
			}
			return false;
		}

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
