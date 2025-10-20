using Flint.Common;
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
			return HashCode.Combine(typeof(This), Hash.Code(Type));
		}

		public override bool Equals(Ast other)
		{
			if (other is This @this)
			{
				return Are.Equal(Type, @this.Type);
			}
			return false;
		}
	}
}
