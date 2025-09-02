namespace Flint.Vm.Cil
{
	class Mul : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Mul(Ast left, Ast right)
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
			return HashCode.Combine(typeof(Mul), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Mul mul)
			{
				return Left.Equals(mul.Left)
					&& Right.Equals(mul.Right);
			}
			return false;
		}
	}
}
