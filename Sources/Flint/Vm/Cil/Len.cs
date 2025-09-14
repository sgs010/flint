using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Len : Ast
	{
		public readonly Ast Array;
		public Len(SequencePoint sp, Ast array) : base(sp)
		{
			Array = array;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Array;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Len), Array);
		}

		public override bool Equals(Ast other)
		{
			if (other is Len len)
			{
				return Array.Equals(len.Array);
			}
			return false;
		}
	}
}
