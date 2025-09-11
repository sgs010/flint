using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Shr : Ast
	{
		public readonly Ast Value;
		public readonly Ast Count;
		public Shr(SequencePoint debug, Ast value, Ast count) : base(debug)
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
			if (other is Shr shr)
			{
				return Value.Equals(shr.Value)
					&& Count.Equals(shr.Count);
			}
			return false;
		}
	}
}
