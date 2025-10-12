namespace Flint.Vm.Cil
{
	class Sub : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Sub(CilPoint pt, Ast left, Ast right) : base(pt)
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
			return HashCode.Combine(typeof(Sub), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Sub sub)
			{
				return Left.Equals(sub.Left)
					&& Right.Equals(sub.Right);
			}
			return false;
		}
	}
}
