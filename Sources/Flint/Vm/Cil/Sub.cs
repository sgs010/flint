using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Sub : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Sub(SequencePoint debug, Ast left, Ast right) : base(debug)
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
