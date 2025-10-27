namespace Flint.Vm.Cil
{
	class Int64 : Literal<long, Int64>
	{
		public Int64(CilPoint pt, long value)
			: base(pt, value) { }
	}
}
