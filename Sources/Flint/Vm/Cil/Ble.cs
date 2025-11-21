namespace Flint.Vm.Cil
{
	class Ble : BinaryOperator<Ble>
	{
		public Ble(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		public static Ble Create(CilPoint pt, Ast left, Ast right)
		{
			return new Ble(pt, left, right);
		}

		protected override Ble CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return Create(pt, left, right);
		}
	}
}
