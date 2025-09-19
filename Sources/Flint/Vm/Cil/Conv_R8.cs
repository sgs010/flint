using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Conv_R8 : Ast
	{
		public readonly Ast Value;
		public Conv_R8(SequencePoint sp, Ast value) : base(sp)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Conv_R8), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Conv_R8 conv)
			{
				return Value.Equals(conv.Value);
			}
			return false;
		}
	}
}
