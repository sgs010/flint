using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Ftn : Ast
	{
		public readonly MethodDefinition Method;
		public Ftn(MethodDefinition mtd)
		{
			Method = mtd;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Ftn), Method);
		}

		public override bool Equals(Ast other)
		{
			if (other is Ftn ftn)
			{
				return Method.Equals(ftn.Method);
			}
			return false;
		}
	}
}
