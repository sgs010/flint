namespace Flint.Vm.Cil
{
	class Null : Ast
	{
		public static readonly Null Instance = new Null();

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override bool Equals(Ast other)
		{
			return other is Null;
		}
	}
}
