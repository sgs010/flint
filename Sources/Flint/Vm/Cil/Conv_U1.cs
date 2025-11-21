namespace Flint.Vm.Cil
{
	class Conv_U1 : UnaryOperator<Conv_U1>
	{
		public Conv_U1(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_U1 CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_U1(pt, value);
		}
	}
}
