namespace Flint.Vm.Cil
{
	class Ceq : BinaryOperator<Ceq>
	{
		public Ceq(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		protected override Ceq CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return new Ceq(pt, left, right);
		}
	}
}
