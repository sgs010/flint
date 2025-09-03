namespace Flint.Vm.Cil
{
	class Refanytype : Ast
	{
		public readonly Ast Reference;
		public Refanytype(Ast reference)
		{
			Reference = reference;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Reference;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Refanytype), Reference);
		}

		public override bool Equals(Ast other)
		{
			if (other is Refanytype @ref)
			{
				return Reference.Equals(@ref.Reference);
			}
			return false;
		}
	}
}
