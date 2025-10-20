using Flint.Common;

namespace Flint.Vm.Cil
{
	class Xor : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Xor(CilPoint pt, Ast left, Ast right) : base(pt)
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
			return HashCode.Combine(typeof(Xor), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Xor xor)
			{
				return Are.Equal(Left, xor.Left)
					&& Are.Equal(Right, xor.Right);
			}
			return false;
		}
	}
}
