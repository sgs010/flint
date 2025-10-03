using Mono.Cecil;

namespace Flint.Common
{
	static class ReflectionExtensions
	{
		#region Interface
		public static bool IsCompilerGenerated(this TypeDefinition type)
		{
			return type.CustomAttributes.Any(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
		}

		public static bool IsCompilerGenerated(this MethodDefinition method)
		{
			return method.CustomAttributes.Any(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
		}

		public static MethodDefinition UnwrapAsyncMethod(this MethodDefinition method)
		{
			// check if method is async and return actual implementation
			MethodDefinition asyncMethod = null;
			if (method.HasCustomAttributes)
			{
				var asyncAttr = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute");
				if (asyncAttr != null)
				{
					var stmType = (TypeDefinition)asyncAttr.ConstructorArguments[0].Value;
					asyncMethod = stmType.Methods.First(x => x.Name == "MoveNext");
				}
			}
			return asyncMethod ?? method;
		}

		public static bool SignatureEquals(this MethodReference left, MethodReference right)
		{
			return left.Name.Equals(right.Name)
				&& left.ReturnType.Equals(right.ReturnType)
				&& left.Parameters.SequenceEqual(right.Parameters, ParameterTypeEqualityComparer.Instance);
		}

		public static bool IsGenericCollection(this TypeReference type, out TypeDefinition itemType, ISet<TypeDefinition> allowedTypes = null)
		{
			// check if type is a System.Collections.Generic.ICollection<T>

			itemType = null;

			if (type.IsGenericInstance == false)
				return false;
			if (type.Namespace != "System.Collections.Generic")
				return false;
			if (type.Name != "ICollection`1")
				return false;

			// get T from System.Collections.Generic.ICollection<T>
			var t = ((GenericInstanceType)type).GenericArguments.First();
			if (allowedTypes != null && allowedTypes.Contains(t) == false)
				return false;

			itemType = t.Resolve();
			return true;
		}

		public static bool IsGenericCollection(this TypeReference type)
		{
			return IsGenericCollection(type, out var _);
		}

		public static bool IsDbSet(this TypeReference type, out TypeDefinition itemType, ISet<TypeDefinition> allowedTypes = null)
		{
			// check if type is a Microsoft.EntityFrameworkCore.DbSet<T>

			itemType = null;

			if (type.IsGenericInstance == false)
				return false;
			if (type.Namespace != "Microsoft.EntityFrameworkCore")
				return false;
			if (type.Name != "DbSet`1")
				return false;

			// get T from DbSet<T>
			var t = ((GenericInstanceType)type).GenericArguments.First();
			if (allowedTypes != null && allowedTypes.Contains(t) == false)
				return false;

			itemType = t.Resolve();
			return true;
		}

		public static bool HasFullName(this MethodReference method, string name)
		{
			return name.Equals(method.DeclaringType.FullName + "." + method.Name, StringComparison.Ordinal);
		}
		#endregion

		#region Implementation
		class ParameterTypeEqualityComparer : IEqualityComparer<ParameterDefinition>
		{
			public static ParameterTypeEqualityComparer Instance = new();

			public bool Equals(ParameterDefinition x, ParameterDefinition y)
			{
				return x.ParameterType.Equals(y.ParameterType);
			}

			public int GetHashCode(ParameterDefinition obj)
			{
				return obj.ParameterType.GetHashCode();
			}
		}
		#endregion
	}
}
