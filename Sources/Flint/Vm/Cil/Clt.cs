namespace Flint.Vm.Cil
{
	class Clt : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Clt(CilPoint pt, Ast left, Ast right) : base(pt)
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
			return HashCode.Combine(typeof(Clt), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Clt clt)
			{
				return Left.Equals(clt.Left)
					&& Right.Equals(clt.Right);
			}
			return false;
		}
	}
}
