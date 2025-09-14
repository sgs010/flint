using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Float32 : Ast
	{
		public readonly float Value;
		public Float32(SequencePoint sp, float value) : base(sp)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Float32), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Float32 float32)
			{
				return Value.Equals(float32.Value);
			}
			return false;
		}
	}
}
