namespace Flint.Vm.Cil
{
	class Not : UnaryOperator<Not>
	{
		public Not(CilPoint pt, Ast value)
			: base(pt, value) { }

		protected override Not CreateInstance(CilPoint pt, Ast value)
		{
			return new Not(pt, value);
		}
	}
}
