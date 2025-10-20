using Flint.Common;
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
			return HashCode.Combine(typeof(Fld), Instance, Hash.Code(Field));
		}

		public override bool Equals(Ast other)
		{
			if (other is Fld fld)
			{
				return Are.Equal(Instance, fld.Instance)
					&& Are.Equal(Field, fld.Field);
			}
			return false;
		}
	}
}
