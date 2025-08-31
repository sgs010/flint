namespace Flint.Vm.Cil
{
	class ArgPtr : Ast
	{
		public readonly int Number;
		public ArgPtr(int number)
		{
			Number = number;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return Number;
		}

		public override bool Equals(Ast other)
		{
			if (other is ArgPtr ptr)
			{
				return Number.Equals(ptr.Number);
			}
			return false;
		}
	}
}
