using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Array : Ast
	{
		public readonly TypeReference Type;
		public readonly Ast Size;
		public Array(CilPoint pt, TypeReference type, Ast size) : base(pt)
		{
			Type = type;
			Size = size;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Size;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Array), Hash.Code(Type), Size);
		}

		public override bool Equals(Ast other)
		{
			if (other is Array array)
			{
				return Are.Equal(Type, array.Type)
					&& Are.Equal(Size, array.Size);
			}
			return false;
		}

		protected override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is Array array)
			{
				if (Are.Equal(Type, array.Type) == false)
					return (null, MergeResult.NotMerged);

				var (size, sizeResult) = Merge(Size, array.Size);
				if (sizeResult == MergeResult.NotMerged)
					return (null, MergeResult.NotMerged);

				var merged = new Array(CilPoint, Type, size);
				return (merged, MergeResult.Merged);
			}
			return (null, MergeResult.NotMerged);
		}
	}
}
