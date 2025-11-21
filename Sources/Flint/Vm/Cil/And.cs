namespace Flint.Vm.Cil
{
	class And : BinaryOperator<And>
	{
		public And(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		protected override And CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return new And(pt, left, right);
		}
	}
}
