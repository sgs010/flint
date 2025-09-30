using System.Text;

namespace Flint.Analyzers
{
	internal class ProjectionAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, AssemblyDefinition asm, string className = null, string methodName = null)
		{
			var entities = EntityAnalyzer.Analyze(asm, className, methodName);
			foreach (var entity in entities)
			{
				if (entity.Properties.Length == 0)
					continue; // no properties accessed
				if (AllProperiesAreAccessed(entity))
					continue; // do not advise a projection if all properties are accessed
				if (EntityAnalyzer.SomePropertiesAreChanged(entity))
					continue; // do not advise a projection if entity is changed

				// report issue
				var sb = new StringBuilder();
				sb.Append("consider using projection { ");
				PrettyPrintEntity(sb, entity, null);
				sb.Append(" } in method ");
				MethodAnalyzer.PrettyPrintMethod(sb, entity.Method, entity.Root.SequencePoint);
				ctx.Log(sb.ToString());
			}
		}
		#endregion

		#region Implementation
		private static bool AllProperiesAreAccessed(EntityDefinition entity)
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

		private static void PrettyPrintEntity(StringBuilder sb, EntityDefinition entity, string prefix)
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
					if (EntityAnalyzer.IsGenericCollection(prop.Property, out var _))
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
