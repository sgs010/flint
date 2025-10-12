namespace Flint.Vm.Cil
{
	class Bge : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Bge(CilPoint pt, Ast left, Ast right) : base(pt)
		{
			Left = left;
			Right = right;
		}

		public static Bge Create(CilPoint pt, Ast left, Ast right)
		{
			return new Bge(pt, left, right);
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Left;
			yield return Right;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Bge), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Bge bge)
			{
				return Left.Equals(bge.Left)
					&& Right.Equals(bge.Right);
			}
			return false;
		}
	}
}
