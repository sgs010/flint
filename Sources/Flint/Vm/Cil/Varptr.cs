using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Varptr : Ast
	{
		public readonly int Index;
		public Varptr(SequencePoint sp, int index) : base(sp)
		{
			Index = index;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Varptr), Index);
		}

		public override bool Equals(Ast other)
		{
			if (other is Varptr v)
			{
				return Index.Equals(v.Index);
			}
			return false;
		}
	}
}
