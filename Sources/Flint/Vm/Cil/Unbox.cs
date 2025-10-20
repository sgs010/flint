using Flint.Common;
using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Unbox : Ast
	{
		public TypeReference Type;
		public readonly Ast Value;
		public Unbox(CilPoint pt, TypeReference type, Ast value) : base(pt)
		{
			Type = type;
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Unbox), Hash.Code(Type), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Unbox unbox)
			{
				return Are.Equal(Type, unbox.Type)
					&& Are.Equal(Value, unbox.Value);
			}
			return false;
		}
	}
}
