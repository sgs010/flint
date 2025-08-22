using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Array : Ast
	{
		public readonly TypeReference Type;
		public readonly Ast Size;
		public readonly Dictionary<Ast, Ast> Elements;
		public Array(TypeReference type, Ast size)
		{
			Type = type;
			Size = size;
			Elements = [];
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Size;
			foreach (var x in Elements)
			{
				yield return x.Key;
				yield return x.Value;
			}
		}

		public override int GetHashCode()
		{
			var hash = new HashCode();
			hash.Add(Type);
			hash.Add(Size);
			foreach (var x in Elements)
			{
				hash.Add(x.Key);
				hash.Add(x.Value);
			}
			return hash.ToHashCode();
		}

		public override bool Equals(Ast other)
		{
			if (other is Array array)
			{
				return Type.Equals(array.Type)
					&& Size.Equals(array.Size)
					&& Elements.SequenceEqual(array.Elements);
			}
			return false;
		}
	}
}
