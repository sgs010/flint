using Flint.Common;

namespace Flint.Vm.Cil
{
	class Add : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Add(CilPoint pt, Ast left, Ast right) : base(pt)
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
			return HashCode.Combine(typeof(Add), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Add add)
			{
				return Are.Equal(Left, add.Left)
					&& Are.Equal(Right, add.Right);
			}
			return false;
		}
	}
}
