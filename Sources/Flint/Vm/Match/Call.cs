using Flint.Common;

namespace Flint.Vm.Match
{
	class Call : Ast
	{
		public readonly Ast Instance;
		public readonly string Method;
		public readonly Ast[] Args;
		public Call(Ast instance, string method, params Ast[] args) : base(null)
		{
			Instance = instance;
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
			return HashCode.Combine(Instance, Method, Args);
		}

		public override bool Equals(Ast other)
		{
			if (other is Cil.Call call)
			{
				return InstanceEquals(this, call)
					&& MethodEquals(this, call)
					&& (Args == Any.Args || Args.SequenceEqual(call.Args));
			}
			return false;
		}

		private static bool InstanceEquals(Match.Call m, Cil.Call c)
		{
			if (m.Instance == Any.Instance)
				return true;
			if (m.Instance == null && c.Instance == null)
				return true;
			if (m.Instance != null && c.Instance != null && m.Instance.Equals(c.Instance))
				return true;
			return false;
		}

		private static bool MethodEquals(Match.Call m, Cil.Call c)
		{
			if (m.Method == null)
				return true;
			if (m.Method.Equals(c.MethodFullName))
				return true;
			if (m.Method.Equals(c.Method.DeclaringType.FullName + "." + c.Method.Name))
				return true;
			if (m.Method.Equals(c.Method.Name))
				return true;
			return false;
		}

		public override void Capture(Ast other, IDictionary<string, Ast> captures)
		{
			if (other is Cil.Call call)
				captures.AddOrReplace(Method ?? call.MethodFullName, call);
		}
	}
}
