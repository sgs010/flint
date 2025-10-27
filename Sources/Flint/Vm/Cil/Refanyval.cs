using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Refanyval : Ast
	{
		public readonly TypeReference Type;
		public readonly Ast Address;
		public Refanyval(CilPoint pt, TypeReference type, Ast address) : base(pt)
		{
			Type = type;
			Address = address;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Address;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Refanyval), Hash.Code(Type), Address);
		}

		public override bool Equals(Ast other)
		{
			if (other is Refanyval @ref)
			{
				return Are.Equal(Type, @ref.Type)
					&& Are.Equal(Address, @ref.Address);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Refanyval @ref)
			{
				if (Are.Equal(Type, @ref.Type) == false)
					return NotMerged();

				var (address, addressMr) = Merge(Address, @ref.Address);
				if (addressMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(new Refanyval(CilPoint, Type, address));
			}
			return NotMerged();
		}
	}
}
