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
		public static IEnumerable<(Instruction, CilPoint)> GetBrs(MethodDefinition method)
		{
			var visitedMethods = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance);
			foreach (var (br, pt) in GetInstructions(method, CilMachine.BranchInstructions, visitedMethods))
				yield return ((Instruction)br.Operand, pt);
		}

		public static IEnumerable<(MethodReference, CilPoint)> GetCalls(MethodDefinition method)
		{
			var visitedMethods = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance);
			foreach (var (call, pt) in GetInstructions(method, CilMachine.CallInstructions, visitedMethods))
				yield return ((MethodReference)call.Operand, pt);
		}

		public static IEnumerable<MethodDefinition> GetMethods(AssemblyInfo asm, string className = null, string methodName = null)
		{
			foreach (var method in asm.MethodInnerCalls.Keys)
			{
				if (methodName != null && method.Name != methodName)
					continue;
				if (className != null && method.DeclaringType.Name != className)
					continue;
				yield return method.Resolve();
			}
		}

		public static ImmutableArray<ImmutableArray<CallInfo>> GetCallChains(AssemblyInfo asm, MethodReference start, string methodLongName)
		{
			if (start == null)
				return [];

			var end = asm.MethodOuterCalls.Keys.Where(x => AssemblyAnalyzer.MethodHasLongName(asm, x, methodLongName)).ToList();
			if (end.Count == 0)
				return [];

			List<ImmutableArray<CallInfo>> chains = [];
			var root = new CallInfo(start, null);
			var visitedMethods = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance);
			foreach (var m in end)
			{
				PopulateCallChains(asm, m, 0, root, null, visitedMethods, chains);
			}
			return [.. chains];
		}

		public static ImmutableArray<ImmutableArray<CallInfo>> GetCallChains(AssemblyInfo asm, MethodReference start, MethodReference end)
		{
			if (start == null)
				return [];
			if (end == null)
				return [];

			List<ImmutableArray<CallInfo>> chains = [];
			var root = new CallInfo(start, null);
			var visitedMethods = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance);
			PopulateCallChains(asm, end, 0, root, null, visitedMethods, chains);
			return [.. chains];
		}

		public static ImmutableArray<ImmutableArray<CallInfo>> GetCallChains(AssemblyInfo asm, MethodReference start, IReadOnlyCollection<MethodReference> ends)
		{
			var result = new List<ImmutableArray<CallInfo>>(ends.Count * 2);
			foreach (var end in ends)
			{
				var chains = GetCallChains(asm, start, end);
				result.AddRange(chains);
			}
			return [.. result];
		}

		public static ImmutableArray<Ast> EvalRaw(MethodDefinition method)
		{
			var actualMethod = method.UnwrapAsyncMethod();

			if (actualMethod.HasBody == false)
				return []; // this is an abstract method, nothing to evaluate

			var expressions = new List<Ast>();

			var methodExpressions = CilMachine.Eval(actualMethod);
			expressions.AddRange(methodExpressions);
			foreach (var ftn in methodExpressions.OfFtn())
			{
				var ftnImpl = ftn.Method.Resolve();
				if (Are.Equal(method, ftnImpl))
					continue; // avoid stack overflow in endless recursion

				var lambdaExpressions = EvalRaw(ftnImpl);
				expressions.AddRange(lambdaExpressions);
			}

			return [.. expressions];
		}

		public static ImmutableArray<Ast> Eval(AssemblyInfo asm, MethodDefinition method)
		{
			if (asm.MethodExpressions.TryGetValue(method, out var expr) == false)
			{
				expr = EvalRaw(method);
				asm.MethodExpressions.Add(method, expr);
			}
			return expr;
		}

		public static string PrettyPrintMethod(MethodReference method)
		{
			var sb = new StringBuilder();
			PrettyPrintMethod(sb, method);
			return sb.ToString();
		}

		public static void PrettyPrintMethod(StringBuilder sb, MethodReference method)
		{
			if (method == null)
				return;

			sb.Append(method.DeclaringType.Namespace);
			sb.Append('.');
			sb.Append(method.DeclaringType.Name);
			sb.Append('.');
			sb.Append(method.Name);
		}

		public static void PrettyPrintMethod(StringBuilder sb, MethodReference method, CilPoint pt)
		{
			PrettyPrintMethod(sb, method);

			if (pt.SequencePoint != null)
			{
				sb.Append(" line ");
				sb.Append(pt.SequencePoint.StartLine);
			}
		}
		#endregion

		#region Implementation
		record CallChainNode(CallChainNode Parent, CallInfo Call);

		private static IEnumerable<(Instruction, CilPoint)> GetInstructions(MethodDefinition method, FrozenSet<Code> codes, HashSet<MethodReference> visitedMethods)
		{
			if (visitedMethods.Contains(method))
				yield break;
			visitedMethods.Add(method);

			var actualMethod = method.UnwrapAsyncMethod();

			if (Are.Equal(method, actualMethod) == false)
			{
				if (visitedMethods.Contains(actualMethod))
					yield break;
				visitedMethods.Add(actualMethod);
			}

			if (actualMethod.HasBody == false)
				yield break;

			// method
			foreach (var x in CilMachine.GetInstructions(actualMethod, codes))
				yield return x;

			// lambdas
			foreach (var (lambda, _) in CilMachine.GetInstructions(actualMethod, CilMachine.LambdaInstructions))
			{
				var lambdaImpl = ((MethodReference)lambda.Operand).Resolve();
				foreach (var call in GetInstructions(lambdaImpl, codes, visitedMethods))
					yield return call;
			}
		}

		private static void PopulateCallChains(AssemblyInfo asm, MethodReference target, int level, CallInfo call, CallChainNode parent, HashSet<MethodReference> visitedMethods, List<ImmutableArray<CallInfo>> chains)
		{
			if (Are.Equal(call.Method, target))
			{
				var chain = new List<CallInfo>(level + 1);
				for (var x = parent; x != null; x = x.Parent)
				{
					chain.Add(x.Call);
				}
				chain.Reverse();
				chains.Add([.. chain]);
			}
			else
			{
				if (asm.MethodInnerCalls.TryGetValue(call.Method, out var innerCalls) == false)
					return;

				foreach (var innerCall in innerCalls)
				{
					if (visitedMethods.Contains(innerCall.Method))
						continue;
					visitedMethods.Add(innerCall.Method);

					if (asm.InterfaceClasses.TryGetValue(innerCall.Method.DeclaringType, out var implTypes))
					{
						// this is an interface method, substitute it with implementations
						foreach (var impl in GetImplMethods(asm, implTypes, innerCall.Method))
						{
							var implCall = new CallInfo(impl, innerCall.CilPoint);
							PopulateCallChains(asm, target, level + 1, implCall, new CallChainNode(parent, implCall), visitedMethods, chains);
						}
					}
					else
					{
						// this is a method call
						PopulateCallChains(asm, target, level + 1, innerCall, new CallChainNode(parent, innerCall), visitedMethods, chains);
					}
				}
			}
		}

		private static ImmutableArray<MethodDefinition> GetImplMethods(AssemblyInfo asm, ImmutableArray<TypeDefinition> implTypes, MethodReference method)
		{
			if (asm.InterfaceMethods.TryGetValue(method, out var implMethods) == false)
			{
				var buf = new List<MethodDefinition>();

				foreach (var t in implTypes)
					foreach (var m in t.Methods)
						if (m.SignatureEquals(method))
							buf.Add(m);

				implMethods = [.. buf];
				asm.InterfaceMethods.Add(method, implMethods);
			}
			return implMethods;
		}
		#endregion
	}
}
