namespace Flint.Vm.Cil
{
	class Bne : BinaryOperator<Bne>
	{
		public Bne(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		public static Bne Create(CilPoint pt, Ast left, Ast right)
		{
			return new Bne(pt, left, right);
		}

		protected override Bne CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return Create(pt, left, right);
		}
	}
}
