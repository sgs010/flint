namespace Flint.Vm.Cil
{
	class Rem : BinaryOperator<Rem>
	{
		public Rem(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		protected override Rem CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return new Rem(pt, left, right);
		}
	}
}
