using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Shl : Ast
	{
		public readonly Ast Value;
		public readonly Ast Count;
		public Shl(SequencePoint sp, Ast value, Ast count) : base(sp)
		{
			Value = value;
			Count = count;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
			yield return Count;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Rem), Value, Count);
		}

		public override bool Equals(Ast other)
		{
			if (other is Shl shl)
			{
				return Value.Equals(shl.Value)
					&& Count.Equals(shl.Count);
			}
			return false;
		}
	}
}
