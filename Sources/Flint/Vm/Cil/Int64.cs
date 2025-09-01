namespace Flint.Vm.Cil
{
	class Int64 : Ast
	{
		public readonly long Value;
		public Int64(long value)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Int64), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Int64 int64)
			{
				return Value.Equals(int64.Value);
			}
			return false;
		}
	}
}
