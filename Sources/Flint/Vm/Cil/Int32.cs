using Flint.Common;

namespace Flint.Vm.Cil
{
	class Int32 : Ast
	{
		public readonly int Value;
		public Int32(CilPoint pt, int value) : base(pt)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Int32), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Int32 int32)
			{
				return Value == int32.Value;
			}
			return false;
		}
	}
}
