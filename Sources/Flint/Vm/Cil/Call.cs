using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Call : Ast
	{
		public readonly Ast Instance;
		public string MethodFullName;
		public readonly MethodReference Method;
		public readonly Ast[] Args;

		public Call(CilPoint pt, Ast instance, MethodReference method, Ast[] args, string methodFullName = null) : base(pt)
		{
			Instance = instance;
			MethodFullName = methodFullName ?? method.FullName;
			Method = method;
			Args = args;
		}

		private Call(CilPoint pt, Ast instance, string methodFullName, MethodReference method, Ast[] args) : base(pt)
		{
			Instance = instance;
			MethodFullName = methodFullName;
			Method = method;
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
					&& Are.Equal(Args, call.Args);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Call call)
			{
				if (Are.Equal(Method, call.Method) == false)
					return NotMerged();

				var (instance, instanceMr) = Merge(Instance, call.Instance);
				if (instanceMr == MergeResult.NotMerged)
					return NotMerged();

				var (args, argsMr) = Merge(Args, call.Args);
				if (argsMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(new Call(CilPoint, instance, MethodFullName, Method, args));
			}
			return NotMerged();
		}
	}
}
