namespace Flint.Vm.Cil
{
	class Add : BinaryOperator<Add>
	{
		public Add(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		protected override Add CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return new Add(pt, left, right);
		}
	}
}
