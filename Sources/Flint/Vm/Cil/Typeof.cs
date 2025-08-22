using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Typeof : Ast
	{
		public readonly TypeReference Type;
		public Typeof(TypeReference type)
		{
			Type = type;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return Type.GetHashCode();
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
