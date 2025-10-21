namespace Flint.Vm.Cil
{
	class Conv_U : UnaryOperator<Conv_U>
	{
		public Conv_U(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_U CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_U(pt, value);
		}
	}
}
