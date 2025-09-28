namespace Flint.Vm
{
	static class AstExtensions
	{
		public static IEnumerable<Ast> OfCall(this Ast expression, string methodName)
		{
			var (captures, ok) = expression.Match(
				new Match.Call(Match.Any.Instance, methodName, Match.Any.Args),
				true);
			if (ok == false)
				yield break;

			foreach (var cap in captures)
				yield return cap.Value;
		}

		public static IEnumerable<Ast> OfCall(this IEnumerable<Ast> expressions, string methodName)
		{
			foreach (var expr in expressions)
				foreach (var call in expr.OfCall(methodName))
					yield return call;
		}
	}
}
