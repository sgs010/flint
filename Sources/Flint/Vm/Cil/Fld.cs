using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Fld : Ast
	{
		public readonly Ast Instance;
		public readonly FieldReference Field;
		public Fld(CilPoint pt, Ast instance, FieldReference fld) : base(pt)
		{
			Instance = instance;
			Field = fld;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Instance;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Fld), Instance, Field);
		}

		public override bool Equals(Ast other)
		{
			if (other is Fld fld)
			{
				return (Instance != null ? Instance.Equals(fld.Instance) : fld.Instance is null)
					&& Field.Equals(fld.Field);
			}
			return false;
		}
	}
}
