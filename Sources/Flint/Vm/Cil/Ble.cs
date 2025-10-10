using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Ble : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Ble(SequencePoint sp, Ast left, Ast right) : base(sp)
		{
			Left = left;
			Right = right;
		}

		public static Ble Create(SequencePoint sp, Ast left, Ast right)
		{
			return new Ble(sp, left, right);
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
