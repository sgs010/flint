namespace Flint.Vm.Cil
{
	class Conv_I4 : UnaryOperator<Conv_I4>
	{
		public Conv_I4(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_I4 CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_I4(pt, value);
		}
	}
}
