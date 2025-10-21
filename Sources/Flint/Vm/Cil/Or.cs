namespace Flint.Vm.Cil
{
	class Or : BinaryOperator<Or>
	{
		public Or(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		protected override Or CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return new Or(pt, left, right);
		}
	}
}
