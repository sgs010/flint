namespace Flint.Vm.Cil
{
	class Cgt : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Cgt(Ast left, Ast right)
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
				return Left.Equals(cgt.Left)
					&& Right.Equals(cgt.Right);
			}
			return false;
		}
	}
}
