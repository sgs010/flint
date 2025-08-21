namespace Flint.Vm.Cil
{
	class String : Ast
	{
		public readonly string Value;
		public String(string value)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override bool Equals(Ast other)
		{
			if (other is String str)
			{
				return Value.Equals(str.Value);
			}
			return false;
		}
	}
}
