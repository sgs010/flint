namespace Flint.Vm.Cil
{
	class Conv_U4 : UnaryOperator<Conv_U4>
	{
		public Conv_U4(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_U4 CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_U4(pt, value);
		}
	}
}
