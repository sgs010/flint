using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Array : Ast
	{
		public readonly TypeReference Type;
		public readonly Ast Size;
		public Array(SequencePoint debug, TypeReference type, Ast size) : base(debug)
		{
			Type = type;
			Size = size;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Size;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Array), Type, Size);
		}

		public override bool Equals(Ast other)
		{
			if (other is Array array)
			{
				return Type.Equals(array.Type)
					&& Size.Equals(array.Size);
			}
			return false;
		}
	}
}
