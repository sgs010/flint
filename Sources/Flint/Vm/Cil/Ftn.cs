using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Ftn : Ast
	{
		public readonly Ast Instance;
		public readonly MethodReference Method;
		public readonly MethodDefinition MethodImpl;
		public Ftn(SequencePoint sp, Ast instance, MethodReference method) : base(sp)
		{
			Instance = instance;
			Method = method;
			MethodImpl = method.Resolve();
		}

		public override IEnumerable<Ast> GetChildren()
		{
			if (Instance != null)
				yield return Instance;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Ftn), Instance, Method, MethodImpl);
		}

		public override bool Equals(Ast other)
		{
			if (other is Ftn ftn)
			{
				return (Instance != null ? Instance.Equals(ftn.Instance) : ftn.Instance is null)
					&& Method.Equals(ftn.Method);
			}
			return false;
		}
	}
}
