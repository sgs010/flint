using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Methodof : Ast
	{
		public readonly MethodReference Method;
		public Methodof(CilPoint pt, MethodReference method) : base(pt)
		{
			Method = method;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Methodof), Hash.Code(Method));
		}

		public override bool Equals(Ast other)
		{
			if (other is Methodof m)
			{
				return Are.Equal(Method, m.Method);
			}
			return false;
		}
	}
}
