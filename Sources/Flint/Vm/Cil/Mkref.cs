using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Mkref : Ast
	{
		public readonly Ast Address;
		public readonly TypeReference Type;
		public Mkref(Ast address, TypeReference type)
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
			return HashCode.Combine(typeof(Mkref), Address, Type);
		}

		public override bool Equals(Ast other)
		{
			if (other is Mkref @ref)
			{
				return Address.Equals(@ref.Address)
					&& Type.Equals(@ref.Type);
			}
			return false;
		}
	}
}
