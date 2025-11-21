namespace Flint.Vm.Cil
{
	class Conv_R4 : UnaryOperator<Conv_R4>
	{
		public Conv_R4(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_R4 CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_R4(pt, value);
		}
	}
}
