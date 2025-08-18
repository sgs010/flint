using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Callvirt : Ast
	{
		public readonly Ast Object;
		public readonly MethodReference Method;
		public readonly Ast[] Args;
		public Callvirt(Ast obj, MethodReference method, Ast[] args)
		{
			Object = obj;
			Method = method;
			Args = args;
		}

		protected override IEnumerable<Ast> GetChildren()
		{
			yield return Object;
			foreach (var arg in Args)
				yield return arg;
		}

		public override bool Equals(Ast other)
		{
			if (other is Callvirt callvirt)
			{
				return Object.Equals(callvirt.Object)
					&& Method.Equals(callvirt.Method)
					&& Args.SequenceEqual(callvirt.Args);
			}
			return false;
		}
	}
}
