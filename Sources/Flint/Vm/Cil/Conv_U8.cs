namespace Flint.Vm.Cil
{
	class Conv_U8 : UnaryOperator<Conv_U8>
	{
		public Conv_U8(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_U8 CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_U8(pt, value);
		}
	}
}
