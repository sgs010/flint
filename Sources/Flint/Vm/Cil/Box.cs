using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Box : Ast
	{
		public readonly Ast Value;
		public Box(SequencePoint sp, Ast value) : base(sp)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Box), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Box box)
			{
				return Value.Equals(box.Value);
			}
			return false;
		}
	}
}
