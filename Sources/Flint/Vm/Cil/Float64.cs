namespace Flint.Vm.Cil
{
	class Float64 : Literal<double, Float64>
	{
		public Float64(CilPoint pt, double value)
			: base(pt, value) { }
	}
}
