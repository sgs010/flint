using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Not : Ast
	{
		public readonly Ast Value;
		public Not(SequencePoint sp, Ast value) : base(sp)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Not), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Not not)
			{
				return Value.Equals(not.Value);
			}
			return false;
		}
	}
}
