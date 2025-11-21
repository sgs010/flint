namespace Flint.Vm.Cil
{
	class Cgt : BinaryOperator<Cgt>
	{
		public Cgt(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		protected override Cgt CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return new Cgt(pt, left, right);
		}
	}
}
