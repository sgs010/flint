namespace Flint.Vm
{
	static class AstExtensions
	{
		public static bool AnyMatch(this IEnumerable<Ast> expressions, Ast pattern, bool recursive = false)
		{
			foreach (var expr in expressions)
			{
				var (_, ok) = expr.Match(pattern, recursive);
				if (ok)
					return true;
			}
			return false;
		}

		public static IEnumerable<Cil.Call> OfCall(this Ast expression, Ast instance, string methodName)
		{
			var (captures, ok) = expression.Match(
				new Match.Call(instance, methodName, Match.Any.Args),
				true);
			if (ok == false)
				yield break;

			foreach (var cap in captures)
				yield return (Cil.Call)cap.Value;
		}

		public static IEnumerable<Cil.Call> OfCall(this Ast expression, string methodName)
		{
			return OfCall(expression, Match.Any.Instance, methodName);
		}

		public static IEnumerable<Cil.Call> OfCall(this Ast expression)
		{
			return OfCall(expression, Match.Any.Instance, null);
		}

		public static IEnumerable<Cil.Call> OfCall(this IEnumerable<Ast> expressions, Ast instance, string methodName)
		{
			foreach (var expr in expressions)
				foreach (var call in expr.OfCall(instance, methodName))
					yield return call;
		}

		public static IEnumerable<Cil.Call> OfCall(this IEnumerable<Ast> expressions, string methodName)
		{
			return OfCall(expressions, Match.Any.Instance, methodName);
		}

		public static IEnumerable<Cil.Call> OfCall(this IEnumerable<Ast> expressions)
		{
			return OfCall(expressions, Match.Any.Instance, null);
		}

		public static IEnumerable<Cil.Ftn> OfFtn(this Ast expression)
		{
			var (captures, ok) = expression.Match(
				new Match.Ftn(),
				true);
			if (ok == false)
				yield break;

			foreach (var cap in captures)
				yield return (Cil.Ftn)cap.Value;
		}

		public static IEnumerable<Cil.Ftn> OfFtn(this IEnumerable<Ast> expressions)
		{
			foreach (var expr in expressions)
				foreach (var ftn in expr.OfFtn())
					yield return ftn;
		}
	}
}
