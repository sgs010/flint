namespace Flint.Vm.Cil
{
	class Clt : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Clt(Ast left, Ast right)
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
			return HashCode.Combine(Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Clt clt)
			{
				return Left.Equals(clt.Left)
					&& Right.Equals(clt.Right);
			}
			return false;
		}
	}
}
