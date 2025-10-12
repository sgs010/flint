namespace Flint.Vm.Cil
{
	class Or : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Or(CilPoint pt, Ast left, Ast right) : base(pt)
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
			return HashCode.Combine(typeof(Or), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Or or)
			{
				return Left.Equals(or.Left)
					&& Right.Equals(or.Right);
			}
			return false;
		}
	}
}
