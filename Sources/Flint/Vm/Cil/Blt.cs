using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Blt : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Blt(SequencePoint sp, Ast left, Ast right) : base(sp)
		{
			Left = left;
			Right = right;
		}

		public static Blt Create(SequencePoint sp, Ast left, Ast right)
		{
			return new Blt(sp, left, right);
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
