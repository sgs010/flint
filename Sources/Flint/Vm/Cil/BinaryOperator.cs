using Flint.Common;

namespace Flint.Vm.Cil
{
	abstract class BinaryOperator<T> : Ast
		where T : BinaryOperator<T>
	{
		public readonly Ast Left;
		public readonly Ast Right;
		protected BinaryOperator(CilPoint pt, Ast left, Ast right) : base(pt)
		{
			Left = left;
			Right = right;
		}

		protected abstract T CreateInstance(CilPoint pt, Ast left, Ast right);

		public sealed override IEnumerable<Ast> GetChildren()
		{
			yield return Left;
			yield return Right;
		}

		public sealed override int GetHashCode()
		{
			return HashCode.Combine(typeof(T), Left, Right);
		}

		public sealed override bool Equals(Ast other)
		{
			if (other is T t)
			{
				return Are.Equal(Left, t.Left)
					&& Are.Equal(Right, t.Right);
			}
			return false;
		}

		protected sealed override (Ast, bool) Merge(Ast other)
		{
			if (other is T t)
			{
				var (left, leftOk) = Merge(Left, t.Left);
				if (leftOk == false)
					return (null, false);

				var (right, rightOk) = Merge(Right, t.Right);
				if (rightOk == false)
					return (null, false);

				var merged = CreateInstance(CilPoint, left, right);
				return (merged, true);
			}
			return (null, false);
		}
	}
}
