namespace Flint.Vm
{
	static class AstExtensions
	{
		public static IEnumerable<Cil.Call> OfCall(this Ast expression, string methodName = null)
		{
			var (captures, ok) = expression.Match(
				new Match.Call(Match.Any.Instance, methodName, Match.Any.Args),
				true);
			if (ok == false)
				yield break;

			foreach (var cap in captures)
				yield return (Cil.Call)cap.Value;
		}

		public static IEnumerable<Cil.Call> OfCall(this IEnumerable<Ast> expressions, string methodName = null)
		{
			foreach (var expr in expressions)
				foreach (var call in expr.OfCall(methodName))
					yield return call;
		}
	}
}
