namespace Flint.Vm.Cil
{
	class Box : Ast
	{
		public readonly Ast Value;
		public Box(Ast value)
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
			if (other is Box box)
			{
				return Value.Equals(box.Value);
			}
			return false;
		}
	}
}
