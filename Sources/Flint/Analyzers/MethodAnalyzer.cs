using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text;
using Flint.Common;
using Flint.Vm;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Analyzers
{
	internal class MethodAnalyzer
	{
		#region Interface
		public static IEnumerable<MethodDefinition> GetMethods(AssemblyDefinition asm, string className = null, string methodName = null)
		{
			foreach (var method in asm.MethodExpressions.Keys)
			{
				if (methodName != null && method.Name != methodName)
					continue;
				if (className != null && method.DeclaringType.Name != className)
					continue;
				yield return method;
			}
		}

		public static HashSet<CallDefinition> GetCalls(AssemblyDefinition asm, MethodReference root, string methodFullName)
		{
			// get all methods in call chains between root and destination methods

			var method = asm.MethodOuterCalls.Keys.Where(x => x.HasFullName(methodFullName)).FirstOrDefault();
			if (method == null)
				return []; // no method found with the given name

			var callsFromRoot = new HashSet<CallDefinition>(CallComparer.Instance);
			CollectCalls(asm.MethodInnerCalls, asm.InterfaceImplementations, root, callsFromRoot);
			if (callsFromRoot.Count == 0)
				return [];

			var callsToMethod = new HashSet<CallDefinition>(CallComparer.Instance);
			CollectCalls(asm.MethodOuterCalls, asm.InterfaceImplementations, method, callsToMethod);
			if (callsToMethod.Count == 0)
				return [];

			callsFromRoot.IntersectWith(callsToMethod);
			return callsFromRoot;
		}

		public static ImmutableArray<Ast> EvalRaw(MethodDefinition method)
		{
			var actualMethod = method.UnwrapAsyncMethod();

			if (actualMethod.HasBody == false)
				return []; // this is an abstract method, nothing to evaluate

			// eval body
			var methodExpressions = CilMachine.Run(actualMethod);

			// eval lambdas
			var lambdaExpressions = methodExpressions.OfFtn().Select(x => EvalRaw(x.MethodImpl)).ToList();

			return methodExpressions.Concat(lambdaExpressions);
		}

		public static ImmutableArray<Ast> Eval(AssemblyDefinition asm, MethodDefinition method)
		{
			if (asm.MethodExpressions.TryGetValue(method, out var expr))
				return expr;
			return [];
		}

		public static void CollectLambdaExpressions(IEnumerable<Ast> methodExpressions, List<Ast> lambdaExpressions)
		{
			// Ftn is ldftn IL instruction, lambdas are translated into this
			foreach (var ftn in methodExpressions.OfFtn())
			{
				var lambdaMethod = ftn.MethodImpl.UnwrapAsyncMethod();
				var lambdaBody = CilMachine.Run(lambdaMethod);
				lambdaExpressions.AddRange(lambdaBody);
				CollectLambdaExpressions(lambdaBody, lambdaExpressions);
			}
		}

		public static void PrettyPrintMethod(StringBuilder sb, MethodDefinition method, SequencePoint sp)
		{
			sb.Append(method.DeclaringType.Namespace);
			sb.Append('.');
			sb.Append(method.DeclaringType.Name);
			sb.Append('.');
			sb.Append(method.Name);

			if (sp != null)
			{
				sb.Append(" line ");
				sb.Append(sp.StartLine);
			}
		}
		#endregion

		#region Implementation
		sealed class CallComparer : IEqualityComparer<CallDefinition>
		{
			public static CallComparer Instance = new();

			public bool Equals(CallDefinition x, CallDefinition y)
			{
				return MethodReferenceEqualityComparer.Equals(x.Method, y.Method);
			}

			public int GetHashCode(CallDefinition obj)
			{
				return MethodReferenceEqualityComparer.GetHashCode(obj.Method);
			}
		}

		private static void CollectCalls(
			FrozenDictionary<MethodReference, ImmutableArray<CallDefinition>> callMap,
			FrozenDictionary<TypeReference, ImmutableArray<TypeDefinition>> interfaceMap,
			MethodReference method,
			HashSet<CallDefinition> calls)
		{
			if (callMap.TryGetValue(method, out var refs) == false)
				return;

			foreach (var r in refs)
			{
				if (calls.Contains(r))
					continue;

				if (interfaceMap.TryGetValue(r.Method.DeclaringType, out var implTypes))
				{
					// this is an interface method, substitute it with implementations
					var implMethods = implTypes.SelectMany(x => x.Methods.Where(m => m.SignatureEquals(r.Method)));
					foreach (var impl in implMethods)
					{
						calls.Add(new CallDefinition { Method = impl, SequencePoint = r.SequencePoint });
						CollectCalls(callMap, interfaceMap, impl, calls);
					}
				}
				else
				{
					// this is a method call
					calls.Add(r);
					CollectCalls(callMap, interfaceMap, r.Method, calls);
				}
			}
		}
		#endregion
	}
}
