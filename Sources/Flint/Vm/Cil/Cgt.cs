using Flint.Common;

namespace Flint.Vm.Cil
{
	class Cgt : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Cgt(CilPoint pt, Ast left, Ast right) : base(pt)
		{
			Left = left;
			Right = right;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Left;
			yield return Right;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Cgt), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Cgt cgt)
			{
				return Are.Equal(Left, cgt.Left)
					&& Are.Equal(Right, cgt.Right);
			}
			return false;
		}
	}
}
