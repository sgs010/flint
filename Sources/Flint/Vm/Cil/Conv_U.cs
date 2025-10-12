namespace Flint.Vm.Cil
{
	class Conv_U : Ast
	{
		public readonly Ast Value;
		public Conv_U(CilPoint pt, Ast value) : base(pt)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Conv_U), Value);
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
