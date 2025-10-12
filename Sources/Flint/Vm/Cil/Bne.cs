namespace Flint.Vm.Cil
{
	class Bne : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Bne(CilPoint pt, Ast left, Ast right) : base(pt)
		{
			Left = left;
			Right = right;
		}

		public static Bne Create(CilPoint pt, Ast left, Ast right)
		{
			return new Bne(pt, left, right);
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Left;
			yield return Right;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Bne), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Bne bne)
			{
				return Left.Equals(bne.Left)
					&& Right.Equals(bne.Right);
			}
			return false;
		}
	}
}
