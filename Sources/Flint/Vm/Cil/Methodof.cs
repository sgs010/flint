using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Methodof : Ast
	{
		public readonly MethodReference Method;
		public Methodof(MethodReference method)
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
			if (other is Methodof m)
			{
				return Method.Equals(m.Method);
			}
			return false;
		}
	}
}
