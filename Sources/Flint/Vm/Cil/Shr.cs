using Flint.Common;

namespace Flint.Vm.Cil
{
	class Shr : Ast
	{
		public readonly Ast Value;
		public readonly Ast Count;
		public Shr(CilPoint pt, Ast value, Ast count) : base(pt)
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
			if (other is Shr shr)
			{
				return Are.Equal(Value, shr.Value)
					&& Are.Equal(Count, shr.Count);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Shr shr)
			{
				var (value, valueMr) = Merge(Value, shr.Value);
				if (valueMr == MergeResult.NotMerged)
					return NotMerged();

				var (count, countMr) = Merge(Count, shr.Count);
				if (countMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(new Shr(CilPoint, value, count));
			}
			return NotMerged();
		}
	}
}
