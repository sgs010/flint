using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Newobj : Ast
	{
		public readonly TypeReference Type;
		public readonly MethodReference Ctor;
		public readonly MethodDefinition CtorImpl;
		public readonly Ast[] Args;
		public Newobj(SequencePoint sp, TypeReference type, MethodReference ctor, Ast[] args) : base(sp)
		{
			Type = type;
			Ctor = ctor;
			CtorImpl = ctor.Resolve();
			Args = args;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			foreach (var arg in Args)
				yield return arg;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Newobj), Type, Ctor, Args);
		}

		public override bool Equals(Ast other)
		{
			if (other is Newobj newobj)
			{
				return Type.Equals(newobj.Type)
					&& Ctor.Equals(newobj.Ctor)
					&& Args.SequenceEqual(newobj.Args);
			}
			return false;
		}
	}
}
