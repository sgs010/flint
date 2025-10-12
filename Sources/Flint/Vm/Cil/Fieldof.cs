using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Fieldof : Ast
	{
		public readonly FieldReference Field;
		public Fieldof(CilPoint pt, FieldReference fld) : base(pt)
		{
			Field = fld;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Fieldof), Field);
		}

		public override bool Equals(Ast other)
		{
			if (other is Fieldof f)
			{
				return Field.Equals(f.Field);
			}
			return false;
		}
	}
}
