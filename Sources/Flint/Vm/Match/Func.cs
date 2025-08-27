namespace Flint.Vm.Match
{
	class Func : Ast
	{
		public static readonly Func Instance = new Func();

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override bool Equals(Ast other)
		{
			if (other is Cil.Func)
			{
				return true;
			}
			return false;
		}

		public override void Capture(Ast other, IDictionary<string, Ast> captures)
		{
			if (other is Cil.Func func)
				captures.Add(func.Method.Name, other);
		}
	}
}
