using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Newobj : Ast
	{
		public readonly TypeReference Type;
		public readonly MethodReference Constructor;
		public readonly Ast[] Args;
		public Newobj(TypeReference type, MethodReference ctor, Ast[] args)
		{
			Type = type;
			Constructor = ctor;
			Args = args;
		}

		protected override IEnumerable<Ast> GetChildren()
		{
			foreach (var arg in Args)
				yield return arg;
		}

		public override bool Equals(Ast other)
		{
			if (other is Newobj newobj)
			{
				return Type.Equals(newobj.Type)
					&& Constructor.Equals(newobj.Constructor)
					&& Args.SequenceEqual(newobj.Args);
			}
			return false;
		}
	}
}
