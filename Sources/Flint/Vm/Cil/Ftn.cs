using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Ftn : Ast
	{
		public readonly Ast Instance;
		public readonly MethodDefinition Method;
		public Ftn(SequencePoint debug, Ast instance, MethodDefinition mtd) : base(debug)
		{
			Instance = instance;
			Method = mtd;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			if (Instance != null)
				yield return Instance;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Ftn), Instance, Method);
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
