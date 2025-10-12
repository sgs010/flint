namespace Flint.Vm.Cil
{
	class Beq : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Beq(CilPoint pt, Ast left, Ast right) : base(pt)
		{
			Left = left;
			Right = right;
		}

		public static Beq Create(CilPoint pt, Ast left, Ast right)
		{
			return new Beq(pt, left, right);
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Left;
			yield return Right;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Beq), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Beq beq)
			{
				return Left.Equals(beq.Left)
					&& Right.Equals(beq.Right);
			}
			return false;
		}
	}
}
