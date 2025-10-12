using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Methodof : Ast
	{
		public readonly MethodReference Method;
		public readonly MethodDefinition MethodImpl;
		public Methodof(CilPoint pt, MethodReference method) : base(pt)
		{
			Method = method;
			MethodImpl = method.Resolve();
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
