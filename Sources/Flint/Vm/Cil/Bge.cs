namespace Flint.Vm.Cil
{
	class Bge : BinaryOperator<Bge>
	{
		public Bge(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		public static Bge Create(CilPoint pt, Ast left, Ast right)
		{
			return new Bge(pt, left, right);
		}

		protected override Bge CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return Create(pt, left, right);
		}
	}
}
