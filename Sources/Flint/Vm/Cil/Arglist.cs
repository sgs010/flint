using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Arglist : Ast
	{
		public readonly MethodDefinition Method;
		public Arglist(SequencePoint sp, MethodDefinition method) : base(sp)
		{
			Method = method;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Arglist), Method);
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
