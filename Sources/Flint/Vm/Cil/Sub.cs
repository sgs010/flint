namespace Flint.Vm.Cil
{
	class Sub : BinaryOperator<Sub>
	{
		public Sub(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		protected override Sub CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return new Sub(pt, left, right);
		}
	}
}
