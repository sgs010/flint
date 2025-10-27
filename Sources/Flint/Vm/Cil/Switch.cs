using Flint.Common;

namespace Flint.Vm.Cil
{
	class Switch : Ast
	{
		public readonly Ast Value;
		public Switch(CilPoint pt, Ast val) : base(pt)
		{
			Value = val;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Switch), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Switch s)
			{
				return Are.Equal(Value, s.Value);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Switch s)
			{
				var (value, valueMr) = Merge(Value, s.Value);
				if (valueMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(new Switch(CilPoint, value));
			}
			return NotMerged();
		}
	}
}
