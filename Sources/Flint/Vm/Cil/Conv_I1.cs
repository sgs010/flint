namespace Flint.Vm.Cil
{
	class Conv_I1 : UnaryOperator<Conv_I1>
	{
		public Conv_I1(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Conv_I1 CreateInstance(CilPoint pt, Ast value)
		{
			return new Conv_I1(pt, value);
		}
	}
}
