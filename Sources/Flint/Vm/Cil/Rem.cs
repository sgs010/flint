namespace Flint.Vm.Cil
{
	class Rem : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Rem(Ast left, Ast right)
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
			return HashCode.Combine(typeof(Rem), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Rem rem)
			{
				return Left.Equals(rem.Left)
					&& Right.Equals(rem.Right);
			}
			return false;
		}
	}
}
