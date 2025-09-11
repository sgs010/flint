using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Conv_U1 : Ast
	{
		public readonly Ast Value;
		public Conv_U1(SequencePoint debug, Ast value) : base(debug)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Conv_U1), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Conv_U1 conv)
			{
				return Value.Equals(conv.Value);
			}
			return false;
		}
	}
}
