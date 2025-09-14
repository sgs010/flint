using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Typeof : Ast
	{
		public readonly TypeReference Type;
		public Typeof(SequencePoint sp, TypeReference type) : base(sp)
		{
			Type = type;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Typeof), Type);
		}

		public override bool Equals(Ast other)
		{
			if (other is Typeof type)
			{
				return Type.Equals(type.Type);
			}
			return false;
		}
	}
}
