namespace Flint.Vm.Cil
{
	class Blt : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Blt(CilPoint pt, Ast left, Ast right) : base(pt)
		{
			Left = left;
			Right = right;
		}

		public static Blt Create(CilPoint pt, Ast left, Ast right)
		{
			return new Blt(pt, left, right);
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Left;
			yield return Right;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Blt), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Blt blt)
			{
				return Left.Equals(blt.Left)
					&& Right.Equals(blt.Right);
			}
			return false;
		}
	}
}
