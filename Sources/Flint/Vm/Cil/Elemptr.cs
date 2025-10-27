using Flint.Common;

namespace Flint.Vm.Cil
{
	class Elemptr : Ast
	{
		public readonly Ast Array;
		public readonly Ast Index;
		public Elemptr(CilPoint pt, Ast array, Ast index) : base(pt)
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
			return HashCode.Combine(typeof(Elemptr), Array, Index);
		}

		public override bool Equals(Ast other)
		{
			if (other is Elemptr ptr)
			{
				return Are.Equal(Array, ptr.Array)
					&& Are.Equal(Index, ptr.Index);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Elemptr elem)
			{
				var (array, arrayMr) = Merge(Array, elem.Array);
				if (arrayMr == MergeResult.NotMerged)
					return NotMerged();

				var (index, indexMr) = Merge(Index, elem.Index);
				if (indexMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(new Elemptr(CilPoint, array, index));
			}
			return NotMerged();
		}
	}
}
