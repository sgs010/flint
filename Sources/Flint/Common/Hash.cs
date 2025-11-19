using Mono.Cecil;

namespace Flint.Common
{
	static class Hash
	{
		public static int Code(TypeReference x)
		{
			if (x == null)
				return 0;
			return x.MetadataToken.GetHashCode();
		}

		public static int Code(MethodReference x)
		{
			if (x == null)
				return 0;
			return x.MetadataToken.GetHashCode();
		}

		public static int Code(ParameterReference x)
		{
			if (x == null)
				return 0;
			return x.MetadataToken.GetHashCode();
		}

		public static int Code(FieldReference x)
		{
			if (x == null)
				return 0;
			return x.MetadataToken.GetHashCode();
		}

		public static int Code(PropertyReference x)
		{
			if (x == null)
				return 0;
			return x.MetadataToken.GetHashCode();
		}
	}
}
