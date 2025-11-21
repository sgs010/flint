namespace Flint.Vm.Cil
{
	class Brtrue : UnaryOperator<Brtrue>
	{
		public Brtrue(CilPoint pt, Ast value)
			: base(pt, value) { }

		public static Brtrue Create(CilPoint pt, Ast val)
		{
			return new Brtrue(pt, val);
		}

		protected override Brtrue CreateInstance(CilPoint pt, Ast value)
		{
			return Create(pt, value);
		}
	}
}
