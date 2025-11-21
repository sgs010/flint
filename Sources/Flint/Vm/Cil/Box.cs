namespace Flint.Vm.Cil
{
	class Box : UnaryOperator<Box>
	{
		public Box(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Box CreateInstance(CilPoint pt, Ast value)
		{
			return new Box(pt, value);
		}
	}
}
