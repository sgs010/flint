using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Mul : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Mul(SequencePoint debug, Ast left, Ast right) : base(debug)
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
			return HashCode.Combine(typeof(Mul), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Mul mul)
			{
				return Left.Equals(mul.Left)
					&& Right.Equals(mul.Right);
			}
			return false;
		}
	}
}
