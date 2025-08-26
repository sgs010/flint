namespace Flint.Vm.Cil
{
	class Varptr : Ast
	{
		public readonly int Index;
		public Varptr(int index)
		{
			Index = index;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return Index;
		}

		public override bool Equals(Ast other)
		{
			if (other is Varptr v)
			{
				return Index.Equals(v.Index);
			}
			return false;
		}
	}
}
