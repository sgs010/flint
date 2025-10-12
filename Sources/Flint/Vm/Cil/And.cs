namespace Flint.Vm.Cil
{
	class And : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public And(CilPoint pt, Ast left, Ast right) : base(pt)
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
			return HashCode.Combine(typeof(And), Left, Right);
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
