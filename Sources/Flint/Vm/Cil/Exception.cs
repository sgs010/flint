namespace Flint.Vm.Cil
{
	class Exception : Ast
	{
		public static readonly Exception Instance = new Exception();

		public Exception() : base(null) { }

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override bool Equals(Ast other)
		{
			return other is Exception;
		}
	}
}
