using Flint.Common;

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
				return Are.Equal(Address, ptr.Address);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Heapptr ptr)
			{
				var (address, addressResult) = Merge(Address, ptr.Address);
				if (addressResult == MergeResult.NotMerged)
					return (null, MergeResult.NotMerged);

				var merged = new Heapptr(CilPoint, address);
				return (merged, MergeResult.Merged);
			}
			return (null, MergeResult.NotMerged);
		}
	}
}
