using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Func : Ast
	{
		public readonly MethodDefinition Method;
		public Func(MethodDefinition mtd)
		{
			Method = mtd;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return Method.GetHashCode();
		}

		public override bool Equals(Ast other)
		{
			if (other is Func func)
			{
				return Method.Equals(func.Method);
			}
			return false;
		}
	}
}
