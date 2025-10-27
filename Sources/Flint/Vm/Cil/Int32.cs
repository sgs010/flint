namespace Flint.Vm.Cil
{
	class Int32 : Literal<int, Int32>
	{
		public Int32(CilPoint pt, int value)
			: base(pt, value) { }
	}
}
