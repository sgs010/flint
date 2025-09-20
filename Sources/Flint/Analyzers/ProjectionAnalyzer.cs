using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Analyzers
{
	internal class ProjectionAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, ModuleDefinition asm, HashSet<TypeReference> entityTypes, string className = null, string methodName = null)
		{
			var entities = EntityAnalyzer.Analyze(asm, entityTypes, className, methodName);
			foreach (var entity in entities)
			{
				if (entity.Properties.Length == 0)
					continue; // no properties accessed
				if (AllProperiesAreAccessed(entity))
					continue; // do not advise a projection if all properties are accessed
				if (SomePropertiesAreChanged(entity))
					continue; // do not advise a projection if entity is changed

				// report issue
				var sb = new StringBuilder();
				sb.Append("consider using projection { ");
				PrettyPrintEntity(sb, entity, null);
				sb.Append(" } in method ");
				PrettyPrintMethod(sb, entity.Method, entity.Root.SequencePoint);
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

		private static bool SomePropertiesAreChanged(EntityDefinition entity)
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

		private static void PrettyPrintMethod(StringBuilder sb, MethodDefinition mtd, SequencePoint sp)
		{
			sb.Append(mtd.DeclaringType.Namespace);
			sb.Append('.');
			sb.Append(mtd.DeclaringType.Name);
			sb.Append('.');
			sb.Append(mtd.Name);

			if (sp != null)
			{
				sb.Append(" line ");
				sb.Append(sp.StartLine);
			}
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
