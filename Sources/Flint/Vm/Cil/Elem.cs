using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Elem : Ast
	{
		public readonly Ast Array;
		public readonly Ast Index;
		public Elem(SequencePoint sp, Ast array, Ast index) : base(sp)
		{
			Array = array;
			Index = index;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Array;
			yield return Index;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Elem), Array, Index);
		}

		public override bool Equals(Ast other)
		{
			if (other is Elem elem)
			{
				return Array.Equals(elem.Array)
					&& Index.Equals(elem.Index);
			}
			return false;
		}
	}
}
