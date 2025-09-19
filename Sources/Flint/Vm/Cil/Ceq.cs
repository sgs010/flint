using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Ceq : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Ceq(SequencePoint sp, Ast left, Ast right) : base(sp)
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
			return HashCode.Combine(typeof(Ceq), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Ceq ceq)
			{
				return Left.Equals(ceq.Left)
					&& Right.Equals(ceq.Right);
			}
			return false;
		}
	}
}
