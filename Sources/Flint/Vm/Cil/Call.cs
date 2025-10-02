using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Call : Ast
	{
		public readonly Ast Instance;
		public readonly MethodReference Method;
		public readonly MethodDefinition MethodImpl;
		public readonly Ast[] Args;
		public Call(SequencePoint sp, Ast instance, MethodReference method, Ast[] args) : base(sp)
		{
			Instance = instance;
			Method = method;
			MethodImpl = method.Resolve();
			Args = args;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			if (Instance != null)
				yield return Instance;
			foreach (var arg in Args)
				yield return arg;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Call), Instance, Method, Args);
		}

		public override bool Equals(Ast other)
		{
			if (other is Call call)
			{
				return (Instance != null ? Instance.Equals(call.Instance) : call.Instance is null)
					&& Method.Equals(call.Method)
					&& Args.SequenceEqual(call.Args);
			}
			return false;
		}
	}
}
