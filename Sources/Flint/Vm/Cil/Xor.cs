namespace Flint.Vm.Cil
{
	class Xor : BinaryOperator<Xor>
	{
		public Xor(CilPoint pt, Ast left, Ast right)
			: base(pt, left, right) { }

		protected override Xor CreateInstance(CilPoint pt, Ast left, Ast right)
		{
			return new Xor(pt, left, right);
		}
	}
}
