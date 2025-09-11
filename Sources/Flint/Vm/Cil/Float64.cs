using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Float64 : Ast
	{
		public readonly double Value;
		public Float64(SequencePoint debug, double value) : base(debug)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Float64), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Float64 float64)
			{
				return Value.Equals(float64.Value);
			}
			return false;
		}
	}
}
