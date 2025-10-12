namespace Flint.Vm.Cil
{
	class Heapptr : Ast
	{
		public readonly Ast Address;
		public Heapptr(CilPoint pt, Ast address) : base(pt)
		{
			Address = address;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Address;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Heapptr), Address);
		}

		public override bool Equals(Ast other)
		{
			if (other is Heapptr ptr)
			{
				return Address.Equals(ptr.Address);
			}
			return false;
		}
	}
}
