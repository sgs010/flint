namespace Flint.Vm.Cil
{
	class Div : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Div(CilPoint pt, Ast left, Ast right) : base(pt)
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
			return HashCode.Combine(typeof(Div), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Div div)
			{
				return Left.Equals(div.Left)
					&& Right.Equals(div.Right);
			}
			return false;
		}
	}
}
