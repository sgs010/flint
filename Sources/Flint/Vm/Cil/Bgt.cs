using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Bgt : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Bgt(SequencePoint sp, Ast left, Ast right) : base(sp)
		{
			Left = left;
			Right = right;
		}

		public static Bgt Create(SequencePoint sp, Ast left, Ast right)
		{
			return new Bgt(sp, left, right);
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
