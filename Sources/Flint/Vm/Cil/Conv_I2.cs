namespace Flint.Vm.Cil
{
	class Conv_I2 : Ast
	{
		public readonly Ast Value;
		public Conv_I2(Ast value)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override bool Equals(Ast other)
		{
			if (other is Conv_I2 i)
			{
				return Value.Equals(i.Value);
			}
			return false;
		}
	}
}
