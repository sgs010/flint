namespace Flint.Vm.Cil
{
	class Blt : BinaryOperator<Blt>
	{
		public Blt(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		public static Blt Create(CilPoint pt, Ast left, Ast right)
		{
			return new Blt(pt, left, right);
		}

		protected override Blt CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return Create(pt, left, right);
		}
	}
}
