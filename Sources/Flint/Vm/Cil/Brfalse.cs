namespace Flint.Vm.Cil
{
	class Brfalse : UnaryOperator<Brfalse>
	{
		public Brfalse(CilPoint pt, Ast value)
			: base(pt, value) { }

		public static Brfalse Create(CilPoint pt, Ast val)
		{
			return new Brfalse(pt, val);
		}

		protected override Brfalse CreateInstance(CilPoint pt, Ast value)
		{
			return Create(pt, value);
		}
	}
}
