using Flint.Common;

namespace Flint.Vm.Cil
{
	class Elem : Ast
	{
		public readonly Ast Array;
		public readonly Ast Index;
		public Elem(CilPoint pt, Ast array, Ast index) : base(pt)
		{
			Array = array;
			Index = index;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Array;
			yield return Index;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Elem), Array, Index);
		}

		public override bool Equals(Ast other)
		{
			if (other is Elem elem)
			{
				return Are.Equal(Array, elem.Array)
					&& Are.Equal(Index, elem.Index);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Elem elem)
			{
				var (array, arrayMr) = Merge(Array, elem.Array);
				if (arrayMr == MergeResult.NotMerged)
					return NotMerged();

				var (index, indexMr) = Merge(Index, elem.Index);
				if (indexMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(new Elem(CilPoint, array, index));
			}
			return NotMerged();
		}
	}
}
