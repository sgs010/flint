using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Call : Ast
	{
		public readonly Ast Instance;
		public string MethodFullName;
		public readonly MethodReference Method;
		public readonly MethodDefinition MethodImpl;
		public readonly Ast[] Args;
		public Call(CilPoint pt, Ast instance, MethodReference method, Ast[] args, string methodFullName = null) : base(pt)
		{
			Instance = instance;
			MethodFullName = methodFullName ?? method.FullName;
			Method = method;
			MethodImpl = method.Resolve();
			Args = args;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Instance;
			foreach (var arg in Args)
				yield return arg;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Call), Instance, Hash.Code(Method), Args);
		}

		public override bool Equals(Ast other)
		{
			if (other is Call call)
			{
				return Are.Equal(Instance, call.Instance)
					&& Are.Equal(Method, call.Method)
					&& Args.SequenceEqual(call.Args);
			}
			return false;
		}
	}
}
