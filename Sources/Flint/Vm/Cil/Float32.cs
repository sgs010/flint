namespace Flint.Vm.Cil
{
	class Float32 : Literal<float, Float32>
	{
		public Float32(CilPoint pt, float value)
			: base(pt, value) { }
	}
}
