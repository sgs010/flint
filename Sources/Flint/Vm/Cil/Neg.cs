using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Neg : Ast
	{
		public readonly Ast Value;
		public Neg(SequencePoint debug, Ast value) : base(debug)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Neg), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Neg neg)
			{
				return Value.Equals(neg.Value);
			}
			return false;
		}
	}
}
