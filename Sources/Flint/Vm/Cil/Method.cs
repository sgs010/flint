using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Method : Ast
	{
		public readonly MethodDefinition Definition;
		public Method(MethodDefinition def)
		{
			Definition = def;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return Definition.GetHashCode();
		}

		public override bool Equals(Ast other)
		{
			if (other is Method m)
			{
				return Definition.Equals(m.Definition);
			}
			return false;
		}
	}
}
