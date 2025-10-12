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
			return HashCode.Combine(typeof(Refanyval), Type, Address);
		}

		public override bool Equals(Ast other)
		{
			if (other is Refanyval @ref)
			{
				return Type.Equals(@ref.Type)
					&& Address.Equals(@ref.Address);
			}
			return false;
		}
	}
}
