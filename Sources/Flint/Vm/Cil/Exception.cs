namespace Flint.Vm.Cil
{
	class Exception : Ast
	{
		public static readonly Exception Instance = new Exception();

		protected override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return Instance.GetHashCode();
		}

		public override bool Equals(Ast other)
		{
			return other is Exception;
		}
	}
}
