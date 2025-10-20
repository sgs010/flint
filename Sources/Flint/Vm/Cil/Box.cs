using Flint.Common;

namespace Flint.Vm.Cil
{
	class Box : Ast
	{
		public readonly Ast Value;
		public Box(CilPoint pt, Ast value) : base(pt)
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
				return Are.Equal(Value, box.Value);
			}
			return false;
		}
	}
}
