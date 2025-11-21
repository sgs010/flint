namespace Flint.Vm.Cil
{
	class Div : BinaryOperator<Div>
	{
		public Div(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		protected override Div CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return new Div(pt, left, right);
		}
	}
}
