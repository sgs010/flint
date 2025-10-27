namespace Flint.Vm.Cil
{
	class String : Literal<string, String>
	{
		public String(CilPoint pt, string value)
			: base(pt, value) { }
	}
}
