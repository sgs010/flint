using Cecil = Mono.Cecil;

namespace Flint.Common
{
	static class CecilExtensions
	{
		#region Interface
		public static bool IsCompilerGenerated(this Cecil.TypeDefinition type)
		{
			return type.CustomAttributes.Any(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
		}

		public static bool IsCompilerGenerated(this Cecil.MethodDefinition method)
		{
			return method.CustomAttributes.Any(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
		}

		public static Cecil.MethodDefinition UnwrapAsyncMethod(this Cecil.MethodDefinition method)
		{
			// check if method is async and return actual implementation
			Cecil.MethodDefinition asyncMethod = null;
			if (method.HasCustomAttributes)
			{
				var asyncAttr = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute");
				if (asyncAttr != null)
				{
					var stmType = (Cecil.TypeDefinition)asyncAttr.ConstructorArguments[0].Value;
					asyncMethod = stmType.Methods.First(x => x.Name == "MoveNext");
				}
			}
			return asyncMethod ?? method;
		}

		public static bool SignatureEquals(this Cecil.MethodDefinition left, Cecil.MethodDefinition right)
		{
			return left.Name.Equals(right.Name)
				&& left.ReturnType.Equals(right.ReturnType)
				&& left.Parameters.SequenceEqual(right.Parameters, ParameterTypeEqualityComparer.Instance);
		}
		#endregion

		#region Implementation
		class ParameterTypeEqualityComparer : IEqualityComparer<Cecil.ParameterDefinition>
		{
			public static ParameterTypeEqualityComparer Instance = new();

			public bool Equals(Cecil.ParameterDefinition x, Cecil.ParameterDefinition y)
			{
				return x.ParameterType.Equals(y.ParameterType);
			}

			public int GetHashCode(Cecil.ParameterDefinition obj)
			{
				return obj.ParameterType.GetHashCode();
			}
		}
		#endregion
	}
}
