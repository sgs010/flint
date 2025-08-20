namespace Flint.Vm.Cil
{
	class Null : Ast
	{
		public static readonly Null Instance = new Null();

		protected override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return Instance.GetHashCode();
		}

		public override bool Equals(Ast other)
		{
			return other is Null;
		}
	}
}
