using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Sizeof : Ast
	{
		public readonly TypeReference Type;
		public Sizeof(CilPoint pt, TypeReference type) : base(pt)
		{
			Type = type;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Sizeof), Hash.Code(Type));
		}

		public override bool Equals(Ast other)
		{
			if (other is Sizeof sz)
			{
				return Are.Equal(Type, sz.Type);
			}
			return false;
		}
	}
}
