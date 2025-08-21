using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Var : Ast
	{
		public readonly int Index;
		public readonly TypeReference Type;
		public Var(int index, TypeReference type)
		{
			Index = index;
			Type = type;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Index, Type);
		}

		public override bool Equals(Ast other)
		{
			if (other is Var v)
			{
				return Index.Equals(v.Index)
					&& Type.Equals(v.Type);
			}
			return false;
		}
	}
}
