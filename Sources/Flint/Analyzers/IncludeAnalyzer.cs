using System.Text;
using Mono.Cecil;
using Match = Flint.Vm.Match;

namespace Flint.Analyzers
{
	internal class IncludeAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, ModuleDefinition asm, HashSet<TypeReference> entityTypes, string className = null, string methodName = null)
		{
			var entities = EntityAnalyzer.Analyze(asm, entityTypes, className, methodName);
			foreach (var entity in entities)
			{
				if (entity.Properties.Length == 0)
					continue; // no properties accessed

				var missingIncludes = new List<Include>();
				Analyze(entity, true, missingIncludes);

				if (missingIncludes.Count == 0)
					continue; // all is ok

				// report issues
				var chains = SplitIntoChains(missingIncludes);
				foreach (var chain in chains)
				{
					var sb = new StringBuilder();
					sb.Append("add ");
					PrettyPrintIncludes(sb, chain);
					sb.Append(" in method ");
					EntityAnalyzer.PrettyPrintMethod(sb, entity.Method, entity.Root.SequencePoint);
					ctx.Log(sb.ToString());
				}
			}
		}
		#endregion

		#region Implementation
		record struct Include(PropertyReference Property, bool TopLevel);

		private static void Analyze(EntityDefinition entity, bool topLevel, List<Include> missingIncludes)
		{
			foreach (var prop in entity.Properties)
			{
				if (prop.Entity == null)
					continue; // skip simple type properties

				var methodName = topLevel
					? "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include"
					: "Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ThenInclude";

				var (_, ok) = entity.Root.Match(
					new Match.Call(null, methodName, Match.Any.Args),
					true);
				if (ok == false)
					missingIncludes.Add(new Include(prop.Property, topLevel));

				Analyze(prop.Entity, false, missingIncludes);
			}
		}

		private static List<List<Include>> SplitIntoChains(List<Include> includes)
		{
			var chains = new List<List<Include>>();
			foreach (var x in includes)
			{
				if (x.TopLevel)
				{
					// start new chain
					chains.Add([x]);
				}
				else
				{
					// add to current chain
					if (chains.Count > 0)
						chains.Last().Add(x);
					else
						chains.Add([x]);
				}
			}
			return chains;
		}

		private static void PrettyPrintIncludes(StringBuilder sb, List<Include> includes)
		{
			var needSeparator = false;
			foreach (var inc in includes)
			{
				if (needSeparator)
					sb.Append('.');
				needSeparator = true;

				var argName = GetShortName(inc.Property.DeclaringType);

				sb.Append(inc.TopLevel ? "Include" : "ThenInclude");
				sb.Append('(');
				sb.Append(argName);
				sb.Append(" => ");
				sb.Append(argName);
				sb.Append('.');
				sb.Append(inc.Property.Name);
				sb.Append(')');
			}
		}

		private static string GetShortName(TypeReference type)
		{
			// convert name "OrderItem" to "oi" and so on

			var sb = new StringBuilder();
			foreach (var symbol in type.Name)
			{
				if (char.IsAsciiLetterUpper(symbol))
					sb.Append(char.ToLowerInvariant(symbol));
			}
			return sb.ToString();
		}
		#endregion
	}
}
