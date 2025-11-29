using System.Text;
using Flint.Common;

namespace Flint.Analyzers
{
	internal class ProjectionAnalyzer
	{
		#region Properties
		public const int Code = 1;
		#endregion

		#region Interface
		public static void Run(IAnalyzerContext ctx, AssemblyInfo asm, string className = null, string methodName = null)
		{
			var queries = QueryAnalyzer.Analyze(asm, className, methodName);
			foreach (var query in queries)
			{
				if (query.Entity.Properties.Length == 0)
					continue; // no properties accessed
				if (AllProperiesAreAccessed(query.Entity))
					continue; // do not advise a projection if all properties are accessed
				if (QueryAnalyzer.SomePropertiesAreChanged(query.Entity))
					continue; // do not advise a projection if entity is changed

				// report issue
				var sb = new StringBuilder();
				sb.Append("consider using projection { ");
				PrettyPrintEntity(sb, query.Entity, null);
				sb.Append(" }");
				ctx.AddResult(Code, sb.ToString(), query.Method, query.CilPoint);
			}
		}
		#endregion

		#region Implementation
		private static bool AllProperiesAreAccessed(EntityInfo entity)
		{
			if (entity.Type.Properties.Count != entity.Properties.Length)
				return false;

			foreach (var prop in entity.Properties)
			{
				if (prop.Entity != null && AllProperiesAreAccessed(prop.Entity) == false)
					return false;
			}
			return true;
		}

		private static void PrettyPrintEntity(StringBuilder sb, EntityInfo entity, string prefix)
		{
			var needSeparator = false;
			foreach (var prop in entity.Properties)
			{
				if (needSeparator)
					sb.Append(", ");
				needSeparator = true;

				if (prefix != null)
					sb.Append(prefix);

				if (prop.Entity == null)
				{
					// property is a simple value (int, string and so on)
					sb.Append(prop.Property.Name);
				}
				else
				{
					if (prop.Property.PropertyType.IsGenericCollection())
					{
						// property is a collection of ONE-TO-MANY entities
						sb.Append(prop.Property.Name);
						sb.Append(" = { ");
						PrettyPrintEntity(sb, prop.Entity, null);
						sb.Append(" }");
					}
					else
					{
						// property is a chained ONE-TO-ONE entity
						PrettyPrintEntity(sb, prop.Entity, prop.Property.Name + ".");
					}
				}
			}
		}
		#endregion
	}
}
