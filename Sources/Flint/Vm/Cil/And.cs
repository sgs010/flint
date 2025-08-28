namespace Flint.Vm.Cil
{
	class And : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public And(Ast left, Ast right)
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
			if (other is And and)
			{
				return Left.Equals(and.Left)
					&& Right.Equals(and.Right);
			}
			return false;
		}
	}
}
