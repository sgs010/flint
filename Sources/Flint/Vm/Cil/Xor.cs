using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Xor : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Xor(SequencePoint sp, Ast left, Ast right) : base(sp)
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
			return HashCode.Combine(typeof(Xor), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Xor xor)
			{
				return Left.Equals(xor.Left)
					&& Right.Equals(xor.Right);
			}
			return false;
		}
	}
}
