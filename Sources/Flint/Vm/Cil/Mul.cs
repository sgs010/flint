namespace Flint.Vm.Cil
{
	class Mul : BinaryOperator<Mul>
	{
		public Mul(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		protected override Mul CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return new Mul(pt, left, right);
		}
	}
}
