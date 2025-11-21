using Flint.Vm;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Common
{
	static class Are
	{
		public static bool Equal(string x, string y)
		{
			if (x == null && y == null)
				return true;
			if (x != null && y != null)
				return string.Equals(x, y);
			return false;
		}

		public static bool Equal(TypeReference x, TypeReference y)
		{
			if (x == null && y == null)
				return true;
			if (x != null && y != null)
				return x.MetadataToken.Equals(y.MetadataToken);
			return false;
		}

		public static bool Equal(MethodReference x, MethodReference y)
		{
			if (x == null && y == null)
				return true;
			if (x != null && y != null)
				return x.MetadataToken.Equals(y.MetadataToken);
			return false;
		}

		public static bool Equal(ParameterReference x, ParameterReference y)
		{
			if (x == null && y == null)
				return true;
			if (x != null && y != null)
				return x.MetadataToken.Equals(y.MetadataToken);
			return false;
		}

		public static bool Equal(FieldReference x, FieldReference y)
		{
			if (x == null && y == null)
				return true;
			if (x != null && y != null)
				return x.MetadataToken.Equals(y.MetadataToken);
			return false;
		}

		public static bool Equal(PropertyReference x, PropertyReference y)
		{
			if (x == null && y == null)
				return true;
			if (x != null && y != null)
				return x.MetadataToken.Equals(y.MetadataToken);
			return false;
		}

		public static bool Equal(SequencePoint x, SequencePoint y)
		{
			if (x == null && y == null)
				return true;
			if (x != null && y != null)
				return x.Equals(y);
			return false;
		}

		public static bool Equal(Ast x, Ast y)
		{
			if (x == null && y == null)
				return true;
			if (x != null && y != null)
				return x.Equals(y);
			return false;
		}
	}
}
