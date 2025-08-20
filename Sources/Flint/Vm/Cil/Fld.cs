using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Fld : Ast
	{
		public readonly Ast Object;
		public readonly FieldReference Field;
		public Fld(Ast obj, FieldReference fld)
		{
			Object = obj;
			Field = fld;
		}

		protected override IEnumerable<Ast> GetChildren()
		{
			yield return Object;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Object, Field);
		}

		public override bool Equals(Ast other)
		{
			if (other is Fld fld)
			{
				return Object.Equals(fld.Object)
					&& Field.Equals(fld.Field);
			}
			return false;
		}
	}
}
