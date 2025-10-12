namespace Flint.Vm.Cil
{
	class Ble : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Ble(CilPoint pt, Ast left, Ast right) : base(pt)
		{
			Left = left;
			Right = right;
		}

		public static Ble Create(CilPoint pt, Ast left, Ast right)
		{
			return new Ble(pt, left, right);
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Left;
			yield return Right;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Ble), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Ble ble)
			{
				return Left.Equals(ble.Left)
					&& Right.Equals(ble.Right);
			}
			return false;
		}
	}
}
