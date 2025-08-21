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
		readonly struct ExpessionMark
		{
			public Ast Expression { get; init; }
			public Ast Mark { get; init; }
		}

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
			var expressions = EvalMachine.Run(mtd);

			var roots = new HashSet<Cil.Call>();
			var marks = new List<ExpessionMark>();
			foreach (var expr in expressions)
			{
				var (captures, ok) = expr.Match(
					new Match.Call(
						null,
						"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync",
						Match.Any.Args),
					true);
				if (ok == false)
					continue;

				var root = (Cil.Call)captures.Values.First();
				roots.Add(root);
				Mark(expr, root, marks);
			}

			var propertyMap = new Dictionary<Ast, HashSet<PropertyReference>>();
			foreach (var root in roots)
			{
				// entity is T from ToListAsync<T>
				var entity = (TypeDefinition)((GenericInstanceMethod)root.Method).GenericArguments.First();
				foreach (var prop in entity.Properties)
				{
					foreach (var mark in marks)
					{
						var (captures, ok) = mark.Expression.Match(
							new Match.Call(Any.Instance, prop.GetMethod.FullName, Any.Args),
							true);
						if (ok == false)
							continue;

						if (propertyMap.TryGetValue(root, out var properties) == false)
						{
							properties = [];
							propertyMap.Add(root, properties);
						}

						properties.Add(prop);
					}
				}
			}

			foreach (var properties in propertyMap.Values)
			{
				var sb = new StringBuilder();
				var needSeparator = false;
				sb.Append("consider using projection { ");
				foreach (var prop in properties)
				{
					if (needSeparator)
						sb.Append(", ");
					sb.Append(prop.Name);
					needSeparator = true;
				}
				sb.Append(" } in method ");
				sb.Append(mtd.DeclaringType.Namespace).Append('.').Append(mtd.DeclaringType.Name).Append('.').Append(mtd.Name).Append("()");
				ctx.Log(sb.ToString());
			}
		}

		private static void Mark(Ast expression, Ast root, List<ExpessionMark> marks)
		{
			if (expression == null)
				return;
			if (expression == root)
				return;

			marks.Add(new ExpessionMark { Expression = expression, Mark = root });
			foreach (var child in expression.GetChildren())
				Mark(child, root, marks);
		}
		#endregion
	}
}
