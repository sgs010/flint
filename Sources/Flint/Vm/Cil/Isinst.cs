using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Isinst : Ast
	{
		public readonly TypeReference Type;
		public readonly Ast Instance;
		public Isinst(CilPoint pt, TypeReference type, Ast instance) : base(pt)
		{
			Type = type;
			Instance = instance;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Instance;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Isinst), Hash.Code(Type), Instance);
		}

		public override bool Equals(Ast other)
		{
			if (other is Isinst inst)
			{
				return Are.Equal(Type, inst.Type)
					&& Are.Equal(Instance, inst.Instance);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Isinst inst)
			{
				if (Are.Equal(Type, inst.Type) == false)
					return NotMerged();

				var (instance, instanceMr) = Merge(Instance, inst.Instance);
				if (instanceMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(new Isinst(CilPoint, Type, instance));
			}
			return NotMerged();
		}
	}
}
