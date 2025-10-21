namespace Flint.Vm.Cil
{
	class Beq : BinaryOperator<Beq>
	{
		public Beq(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		public static Beq Create(CilPoint pt, Ast left, Ast right)
		{
			return new Beq(pt, left, right);
		}

		protected override Beq CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return Create(pt, left, right);
		}
	}
}
