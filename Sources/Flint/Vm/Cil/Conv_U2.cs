using Flint.Common;

namespace Flint.Vm.Cil
{
	class Conv_U2 : Ast
	{
		public readonly Ast Value;
		public Conv_U2(CilPoint pt, Ast value) : base(pt)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Conv_U2), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Conv_U2 conv)
			{
				return Are.Equal(Value, conv.Value);
			}
			return false;
		}
	}
}
