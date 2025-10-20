using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Ftn : Ast
	{
		public readonly Ast Instance;
		public readonly MethodReference Method;
		public readonly MethodDefinition MethodImpl;
		public Ftn(CilPoint pt, Ast instance, MethodReference method) : base(pt)
		{
			Instance = instance;
			Method = method;
			MethodImpl = method.Resolve();
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Instance;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Ftn), Instance, Hash.Code(Method));
		}

		public override bool Equals(Ast other)
		{
			if (other is Ftn ftn)
			{
				return Are.Equal(Instance, ftn.Instance)
					&& Are.Equal(Method, ftn.Method);
			}
			return false;
		}
	}
}
