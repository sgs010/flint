namespace Flint.Vm.Cil
{
	class Unaligned : Ast
	{
		public readonly byte Address;
		public Unaligned(CilPoint pt, byte address) : base(pt)
		{
			Address = address;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Unaligned), Address);
		}

		public override bool Equals(Ast other)
		{
			if (other is Unaligned un)
			{
				return Address == un.Address;
			}
			return false;
		}
	}
}
