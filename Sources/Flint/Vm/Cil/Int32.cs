namespace Flint.Vm.Cil
{
	class Int32 : Ast
	{
		public readonly int Value;
		public Int32(int value)
		{
			Value = value;
		}

		protected override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return Value;
		}

		public override bool Equals(Ast other)
		{
			if (other is Int32 int32)
			{
				return Value.Equals(int32.Value);
			}
			return false;
		}
	}
}
