namespace Flint.Vm.Cil
{
	class Clt : BinaryOperator<Clt>
	{
		public Clt(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		protected override Clt CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return new Clt(pt, left, right);
		}
	}
}
