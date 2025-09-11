using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class And : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public And(SequencePoint debug, Ast left, Ast right) : base(debug)
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
			return HashCode.Combine(typeof(And), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is And and)
			{
				return Left.Equals(and.Left)
					&& Right.Equals(and.Right);
			}
			return false;
		}
	}
}
