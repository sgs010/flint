namespace Flint.Vm.Cil
{
	class Varptr : Ast
	{
		public readonly int Index;
		public Varptr(CilPoint pt, int index) : base(pt)
		{
			Index = index;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Varptr), Index);
		}

		public override bool Equals(Ast other)
		{
			if (other is Varptr v)
			{
				return Index == v.Index;
			}
			return false;
		}
	}
}
