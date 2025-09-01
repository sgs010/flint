namespace Flint.Vm.Cil
{
	class Len : Ast
	{
		public readonly Ast Array;
		public Len(Ast array)
		{
			Array = array;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Array;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Len), Array);
		}

		public override bool Equals(Ast other)
		{
			if (other is Len len)
			{
				return Array.Equals(len.Array);
			}
			return false;
		}
	}
}
