namespace Flint.Vm.Match
{
	class Any : Ast
	{
		public static readonly Ast Instance = new Any();
		public static readonly Ast[] Args = [];

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override bool Equals(Ast other)
		{
			return true;
		}
	}
}
