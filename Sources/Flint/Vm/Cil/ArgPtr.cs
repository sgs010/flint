namespace Flint.Vm.Cil
{
	class Argptr : Ast
	{
		public readonly int Number;
		public Argptr(CilPoint pt, int number) : base(pt)
		{
			Number = number;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Argptr), Number);
		}

		public override bool Equals(Ast other)
		{
			if (other is Argptr ptr)
			{
				return Number == ptr.Number;
			}
			return false;
		}
	}
}
