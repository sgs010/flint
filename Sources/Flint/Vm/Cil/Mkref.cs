using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Mkref : Ast
	{
		public readonly Ast Address;
		public readonly TypeReference Type;
		public Mkref(CilPoint pt, Ast address, TypeReference type) : base(pt)
		{
			Address = address;
			Type = type;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Address;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Mkref), Address, Hash.Code(Type));
		}

		public override bool Equals(Ast other)
		{
			if (other is Mkref @ref)
			{
				return Are.Equal(Address, @ref.Address)
					&& Are.Equal(Type, @ref.Type);
			}
			return false;
		}
	}
}
