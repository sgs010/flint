using Flint.Common;

namespace Flint.Vm.Match
{
	class Ftn : Ast
	{
		public static readonly Ftn Instance = new Ftn();

		public Ftn() : base(null) { }

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override bool Equals(Ast other)
		{
			if (other is Cil.Ftn)
			{
				return true;
			}
			return false;
		}

		public override void Capture(Ast other, IDictionary<string, Ast> captures)
		{
			if (other is Cil.Ftn func)
				captures.AddOrReplace(func.Method.Name, other);
		}
	}
}
