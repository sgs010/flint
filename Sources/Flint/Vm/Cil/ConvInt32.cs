namespace Flint.Vm.Cil
{
	class ConvInt32 : Ast
	{
		public readonly Ast Value;
		public ConvInt32(Ast value)
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
			if (other is ConvInt32 conv)
			{
				return Value.Equals(conv.Value);
			}
			return false;
		}
	}
}
