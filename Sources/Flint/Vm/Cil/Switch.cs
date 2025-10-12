namespace Flint.Vm.Cil
{
	class Switch : Ast
	{
		public readonly Ast Value;
		public Switch(CilPoint pt, Ast val) : base(pt)
		{
			Value = val;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Switch), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Switch s)
			{
				return Value.Equals(s.Value);
			}
			return false;
		}
	}
}
