namespace Flint.Vm.Cil
{
	class Conv_U2 : UnaryOperator<Conv_U2>
	{
		public Conv_U2(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_U2 CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_U2(pt, value);
		}
	}
}
