namespace Flint.Vm.Cil
{
	class Bgt : BinaryOperator<Bgt>
	{
		public Bgt(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		public static Bgt Create(CilPoint pt, Ast left, Ast right)
		{
			return new Bgt(pt, left, right);
		}

		protected override Bgt CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return Create(pt, left, right);
		}
	}
}
