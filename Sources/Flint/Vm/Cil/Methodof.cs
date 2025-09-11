using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Methodof : Ast
	{
		public readonly MethodReference Method;
		public Methodof(SequencePoint debug, MethodReference method) : base(debug)
		{
			Method = method;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Methodof), Method);
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
