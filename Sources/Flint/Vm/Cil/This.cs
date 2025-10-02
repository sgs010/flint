using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class This : Ast
	{
		public readonly TypeDefinition Type;
		public This(SequencePoint sp, TypeDefinition type) : base(sp)
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
