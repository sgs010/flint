namespace Flint.Vm.Cil
{
	class Conv_R8 : UnaryOperator<Conv_R8>
	{
		public Conv_R8(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_R8 CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_R8(pt, value);
		}
	}
}
