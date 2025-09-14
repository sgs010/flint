using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class This : Ast
	{
		public readonly TypeReference Type;
		public This(SequencePoint sp, TypeReference type) : base(sp)
		{
			Type = type;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(This), Type);
		}

		public override bool Equals(Ast other)
		{
			if (other is This @this)
			{
				return Type.Equals(@this.Type);
			}
			return false;
		}
	}
}
