namespace Flint.Vm.Cil
{
	class Conv_I2 : UnaryOperator<Conv_I2>
	{
		public Conv_I2(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_I2 CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_I2(pt, value);
		}
	}
}
