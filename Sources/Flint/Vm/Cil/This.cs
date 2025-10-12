using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class This : Ast
	{
		public readonly TypeDefinition Type;
		public This(CilPoint pt, TypeDefinition type) : base(pt)
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
