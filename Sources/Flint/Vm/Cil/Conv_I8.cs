namespace Flint.Vm.Cil
{
	class Conv_I8 : UnaryOperator<Conv_I8>
	{
		public Conv_I8(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_I8 CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_I8(pt, value);
		}
	}
}
