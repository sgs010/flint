namespace Flint.Vm.Cil
{
	class Conv_I : UnaryOperator<Conv_I>
	{
		public Conv_I(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_I CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_I(pt, value);
		}
	}
}
