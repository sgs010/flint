using System.Text;
using Flint.Common;
using Flint.Vm;
using Flint.Vm.Match;
using Mono.Cecil;
using Cil = Flint.Vm.Cil;
using Match = Flint.Vm.Match;

namespace Flint.Analyzers
{
	internal class ProjectionAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, ModuleDefinition asm, string className = null, string methodName = null)
		{
			foreach (var mtd in GetMethods(asm, className, methodName))
			{
				Analyze(ctx, mtd);
			}
		}
		#endregion

		#region Implementation
		private static IEnumerable<MethodDefinition> GetMethods(ModuleDefinition asm, string className = null, string methodName = null)
		{
			foreach (var type in asm.Types)
			{
				if (className != null && type.Name != className)
					continue;

				foreach (var mtd in type.Methods)
				{
					if (methodName != null && mtd.Name != methodName)
						continue;

					yield return mtd;
				}
			}
		}

		private static void Analyze(IAnalyzerContext ctx, MethodDefinition mtd)
		{
			// eval method body
			var expressions = EvalMachine.Run(mtd);

			// find roots (methods where IQueryable monad is unwrapped like ToListAsync)
			// for every found root mark every ast accessible from it
			var roots = new HashSet<Cil.Call>();
			var marks = new Dictionary<Ast, List<Ast>>();
			foreach (var expr in expressions)
			{
				var (root, ok) = CaptureAnyRoot(expr,
				[
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync",
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync",
				]);
				if (ok == false)
					continue;

				roots.Add(root);
				var rootExpressions = marks.GetValueOrAddNew(root);
				Mark(expr, root, rootExpressions);
			}

			// gather accessed properties
			var propertyMap = new Dictionary<Ast, HashSet<PropertyReference>>();
			foreach (var root in roots)
			{
				if (marks.TryGetValue(root, out var rootExpressions) == false)
					continue;

				// entity is T from METHOD<T> (i.e. ToListAsync<T>)
				var entity = (TypeDefinition)((GenericInstanceMethod)root.Method).GenericArguments.First();
				foreach (var prop in entity.Properties)
				{
					foreach (var expr in rootExpressions)
					{
						var (captures, ok) = expr.Match(
							new Match.Call(Any.Instance, prop.GetMethod.FullName, Any.Args),
							true);
						if (ok == false)
							continue;

						var properties = propertyMap.GetValueOrAddNew(root);
						properties.Add(prop);
					}
				}
			}

			// report issues
			foreach (var properties in propertyMap.Values)
			{
				var instance = properties.First().DeclaringType.Resolve();
				if (instance.Properties.Count == properties.Count)
					continue; // do not advise a projection if all properties are accessed

				var sb = new StringBuilder();
				sb.Append("consider using projection { ");
				properties.PrettyPrint(sb, ", ", x => x.Name);
				sb.Append(" } in method ");
				mtd.PrettyPrint(sb);
				ctx.Log(sb.ToString());
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
		#endregion
	}
}
