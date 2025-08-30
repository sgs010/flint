namespace Flint.Vm.Cil
{
	class Conv_U : Ast
	{
		public readonly Ast Value;
		public Conv_U(Ast value)
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
			if (other is Conv_U conv)
			{
				return Value.Equals(conv.Value);
			}
			return false;
		}
	}
}
