using Flint.Common;

namespace Flint.Vm.Cil
{
	class Shl : Ast
	{
		public readonly Ast Value;
		public readonly Ast Count;
		public Shl(CilPoint pt, Ast value, Ast count) : base(pt)
		{
			Value = value;
			Count = count;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
			yield return Count;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Rem), Value, Count);
		}

		public override bool Equals(Ast other)
		{
			if (other is Shl shl)
			{
				return Are.Equal(Value, shl.Value)
					&& Are.Equal(Count, shl.Count);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Shl shl)
			{
				var (value, valueMr) = Merge(Value, shl.Value);
				if (valueMr == MergeResult.NotMerged)
					return NotMerged();

				var (count, countMr) = Merge(Count, shl.Count);
				if (countMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(new Shl(CilPoint, value, count));
			}
			return NotMerged();
		}
	}
}
