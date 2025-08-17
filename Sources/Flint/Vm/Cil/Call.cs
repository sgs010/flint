using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Call : Ast
	{
		public readonly MethodReference Method;
		public readonly Ast[] Args;
		public Call(MethodReference method, Ast[] args)
		{
			Method = method;
			Args = args;
		}

		protected override IEnumerable<Ast> GetChildren()
		{
			foreach (var arg in Args)
				yield return arg;
		}

		public override bool Equals(Ast other)
		{
			if (other is Call call)
			{
				return Method.Equals(call.Method)
					&& Args.SequenceEqual(call.Args);
			}
			return false;
		}
	}
}
