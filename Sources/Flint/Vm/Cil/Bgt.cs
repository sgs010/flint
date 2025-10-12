namespace Flint.Vm.Cil
{
	class Bgt : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Bgt(CilPoint pt, Ast left, Ast right) : base(pt)
		{
			Left = left;
			Right = right;
		}

		public static Bgt Create(CilPoint pt, Ast left, Ast right)
		{
			return new Bgt(pt, left, right);
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Left;
			yield return Right;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Bgt), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Bgt bgt)
			{
				return Left.Equals(bgt.Left)
					&& Right.Equals(bgt.Right);
			}
			return false;
		}
	}
}
