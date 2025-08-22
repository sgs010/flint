using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Type : Ast
	{
		public readonly TypeDefinition Definition;
		public Type(TypeDefinition def)
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
			if (other is Type type)
			{
				return Definition.Equals(type.Definition);
			}
			return false;
		}
	}
}
