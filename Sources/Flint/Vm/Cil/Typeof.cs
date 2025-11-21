using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Typeof : Ast
	{
		public readonly TypeReference Type;
		public Typeof(CilPoint pt, TypeReference type) : base(pt)
		{
			Type = type;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Typeof), Hash.Code(Type));
		}

		public override bool Equals(Ast other)
		{
			if (other is Typeof type)
			{
				return Are.Equal(Type, type.Type);
			}
			return false;
		}
	}
}
