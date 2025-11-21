namespace Flint.Vm.Cil
{
	class Neg : UnaryOperator<Neg>
	{
		public Neg(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Neg CreateInstance(CilPoint pt, Ast value)
		{
			return new Neg(pt, value);
		}
	}
}
