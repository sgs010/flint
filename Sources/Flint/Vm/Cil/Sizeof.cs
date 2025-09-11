using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Sizeof : Ast
	{
		public readonly TypeReference Type;
		public Sizeof(SequencePoint debug, TypeReference type) : base(debug)
		{
			Type = type;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Sizeof), Type);
		}

		public override bool Equals(Ast other)
		{
			if (other is Sizeof sz)
			{
				return Type.Equals(sz.Type);
			}
			return false;
		}
	}
}
