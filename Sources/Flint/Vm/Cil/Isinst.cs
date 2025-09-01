using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Isinst : Ast
	{
		public readonly TypeReference Type;
		public readonly Ast Instance;
		public Isinst(TypeReference type, Ast instance)
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
			return HashCode.Combine(typeof(Isinst), Type, Instance);
		}

		public override bool Equals(Ast other)
		{
			if (other is Isinst inst)
			{
				return Type.Equals(inst.Type)
					&& Instance.Equals(inst.Instance);
			}
			return false;
		}
	}
}
