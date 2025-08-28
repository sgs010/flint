using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class IsInstance : Ast
	{
		public readonly TypeReference Type;
		public readonly Ast Instance;
		public IsInstance(TypeReference type, Ast instance)
		{
			Type = type;
			Instance = instance;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Instance;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Type, Instance);
		}

		public override bool Equals(Ast other)
		{
			if (other is IsInstance inst)
			{
				return Type.Equals(inst.Type)
					&& Instance.Equals(inst.Instance);
			}
			return false;
		}
	}
}
