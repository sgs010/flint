using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Castclass : Ast
	{
		public readonly TypeReference Type;
		public readonly Ast Value;
		public Castclass(SequencePoint debug, TypeReference type, Ast value) : base(debug)
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
			return HashCode.Combine(typeof(Castclass), Type, Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Castclass cast)
			{
				return Type.Equals(cast.Type)
					&& Value.Equals(cast.Value);
			}
			return false;
		}
	}
}
