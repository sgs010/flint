using Flint.Common;

namespace Flint.Vm.Cil
{
	class Clt : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Clt(CilPoint pt, Ast left, Ast right) : base(pt)
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
			return HashCode.Combine(typeof(Clt), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Clt clt)
			{
				return Are.Equal(Left, clt.Left)
					&& Are.Equal(Right, clt.Right);
			}
			return false;
		}
	}
}
