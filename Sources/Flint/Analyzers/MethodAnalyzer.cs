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

			var callsFromRoot = new HashSet<CallDefinition>();
			CollectCalls(asm.MethodInnerCalls, asm.InterfaceImplementations, root, callsFromRoot);
			if (callsFromRoot.Count == 0)
				return [];

			var callsToMethod = new HashSet<CallDefinition>();
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

		//public static List<Ast> EvalRecursive(AssemblyDefinition asm, MethodDefinition method)
		//{
		//	var methodMap = new Dictionary<MethodDefinition, List<Ast>>();
		//	EvalRecursive(asm, method, methodMap);
		//	return methodMap.Values.SelectMany(x => x).ToList();
		//}

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

		//private static void EvalRecursive(AssemblyDefinition asm, MethodDefinition method, Dictionary<MethodDefinition, List<Ast>> methodMap)
		//{
		//	if (methodMap.ContainsKey(method))
		//		return; // already evaluated

		//	List<Ast> methodExpressions = [];
		//	if (method.HasBody)
		//	{
		//		// eval method
		//		var expr = Eval(asm, method);
		//		methodExpressions.AddRange(expr);
		//	}
		//	else if (method.DeclaringType.IsInterface)
		//	{
		//		// try to eval interface implementations
		//		if (asm.InterfaceImplementations.TryGetValue(method.DeclaringType, out var implTypes))
		//		{
		//			var implMethods = implTypes.SelectMany(x => x.Methods.Where(m => m.SignatureEquals(method)));
		//			foreach (var impl in implMethods)
		//			{
		//				var expr = Eval(asm, impl);
		//				methodExpressions.AddRange(expr);
		//			}
		//		}
		//	}
		//	if (methodExpressions.Count == 0)
		//		return; // nothing to evaluate

		//	methodMap.Add(method, methodExpressions);
		//	foreach (var call in methodExpressions.OfCall())
		//	{
		//		if (call.Method.Module != asm.Module)
		//			continue; // do not evaluate external modules

		//		var callMethod = call.MethodImpl;
		//		if (callMethod.IsCompilerGenerated())
		//			continue; // do not evaluate get/set autogenerated methods

		//		EvalRecursive(asm, callMethod, methodMap);
		//	}
		//}
		#endregion
	}
}
