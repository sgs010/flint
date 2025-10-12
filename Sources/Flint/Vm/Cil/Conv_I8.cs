namespace Flint.Vm.Cil
{
	class Conv_I8 : Ast
	{
		public readonly Ast Value;
		public Conv_I8(CilPoint pt, Ast value) : base(pt)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Conv_I8), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Conv_I8 conv)
			{
				return Value.Equals(conv.Value);
			}
			return false;
		}
	}
}
