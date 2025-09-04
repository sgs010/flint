using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Unbox : Ast
	{
		public TypeReference Type;
		public readonly Ast Value;
		public Unbox(TypeReference type, Ast value)
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
			return HashCode.Combine(typeof(Unbox), Type, Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Unbox unbox)
			{
				return Type.Equals(unbox.Type)
					&& Value.Equals(unbox.Value);
			}
			return false;
		}
	}
}
