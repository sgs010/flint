using System.Collections.Frozen;
using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Match
{
	class CallMap : Ast
	{
		public readonly FrozenSet<MethodReference> MethodMap;
		public CallMap(IEnumerable<MethodReference> methods) : base(null)
		{
			MethodMap = methods.ToFrozenSet(MethodReferenceEqualityComparer.Instance);
		}

		public CallMap(FrozenSet<MethodReference> methods) : base(null)
		{
			MethodMap = methods;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(CallMap), MethodMap);
		}

		public override bool Equals(Ast other)
		{
			if (other is Cil.Call call)
			{
				return MethodMap.Contains(call.Method);
			}
			return false;
		}

		public override void Capture(Ast other, IDictionary<string, Ast> captures)
		{
			if (other is Cil.Call call)
				captures.AddOrReplace(call.MethodFullName, call);
		}
	}
}
