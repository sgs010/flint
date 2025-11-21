using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Arglist : Ast
	{
		public readonly MethodDefinition Method;
		public Arglist(CilPoint pt, MethodDefinition method) : base(pt)
		{
			Method = method;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Arglist), Hash.Code(Method));
		}

		public override bool Equals(Ast other)
		{
			if (other is Arglist arglist)
			{
				return Are.Equal(Method, arglist.Method);
			}
			return false;
		}
	}
}
