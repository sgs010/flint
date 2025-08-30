namespace Flint.Vm.Cil
{
	class Conv_I : Ast
	{
		public readonly Ast Value;
		public Conv_I(Ast value)
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
			if (other is Conv_I conv)
			{
				return Value.Equals(conv.Value);
			}
			return false;
		}
	}
}
