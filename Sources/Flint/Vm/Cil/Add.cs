using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Add : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Add(SequencePoint debug, Ast left, Ast right) : base(debug)
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
			return HashCode.Combine(typeof(Add), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Add add)
			{
				return Left.Equals(add.Left)
					&& Right.Equals(add.Right);
			}
			return false;
		}
	}
}
