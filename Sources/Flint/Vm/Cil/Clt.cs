using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Clt : Ast
	{
		public readonly Ast Left;
		public readonly Ast Right;
		public Clt(SequencePoint sp, Ast left, Ast right) : base(sp)
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
			return HashCode.Combine(typeof(Clt), Left, Right);
		}

		public override bool Equals(Ast other)
		{
			if (other is Clt clt)
			{
				return Left.Equals(clt.Left)
					&& Right.Equals(clt.Right);
			}
			return false;
		}
	}
}
