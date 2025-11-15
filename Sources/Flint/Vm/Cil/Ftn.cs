using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Ftn : Ast
	{
		public readonly Ast Instance;
		public readonly MethodReference Method;
		public Ftn(CilPoint pt, Ast instance, MethodReference method) : base(pt)
		{
			Instance = instance;
			Method = method;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Instance;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Ftn), Instance, Hash.Code(Method));
		}

		public override bool Equals(Ast other)
		{
			if (other is Ftn ftn)
			{
				return Are.Equal(Instance, ftn.Instance)
					&& Are.Equal(Method, ftn.Method);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Ftn ftn)
			{
				if (Are.Equal(Method, ftn.Method) == false)
					return NotMerged();

				var (instance, instanceMr) = Merge(Instance, ftn.Instance);
				if (instanceMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(new Ftn(CilPoint, instance, Method));
			}
			return NotMerged();
		}
	}
}
