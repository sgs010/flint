using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Arglist : Ast
	{
		public readonly MethodDefinition Method;
		public Arglist(MethodDefinition method)
		{
			Method = method;
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
			if (other is Arglist arglist)
			{
				return Method.Equals(arglist.Method);
			}
			return false;
		}
	}
}
