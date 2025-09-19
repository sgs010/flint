using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Argptr : Ast
	{
		public readonly int Number;
		public Argptr(SequencePoint sp, int number) : base(sp)
		{
			Number = number;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Argptr), Number);
		}

		public override bool Equals(Ast other)
		{
			if (other is Argptr ptr)
			{
				return Number.Equals(ptr.Number);
			}
			return false;
		}
	}
}
