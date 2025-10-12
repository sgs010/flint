using System.Collections.Immutable;
using System.Text;
using Flint.Common;
using Flint.Vm;
using Mono.Cecil;

namespace Flint.Analyzers
{
	internal class MethodAnalyzer
	{
		#region Interface
		public static IEnumerable<MethodDefinition> GetMethods(AssemblyInfo asm, string className = null, string methodName = null)
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

		public static List<List<CallInfo>> GetCallChains(AssemblyInfo asm, MethodReference start, string methodFullName)
		{
			if (start == null)
				return [];

			var end = asm.MethodOuterCalls.Keys.Where(x => x.HasFullName(methodFullName)).ToList();
			if (end.Count == 0)
				return [];

			List<List<CallInfo>> chains = [];
			var root = new CallInfo(start, null);
			var visitedMethods = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance);
			foreach (var m in end)
			{
				PopulateCallChains(asm, m, 0, root, null, visitedMethods, chains);
			}
			return chains;
		}

		public static List<List<CallInfo>> GetCallChains(AssemblyInfo asm, MethodReference start, MethodReference end)
		{
			if (start == null)
				return [];
			if (end == null)
				return [];

			List<List<CallInfo>> chains = [];
			var root = new CallInfo(start, null);
			var visitedMethods = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance);
			PopulateCallChains(asm, end, 0, root, null, visitedMethods, chains);
			return chains;
		}

		public static ImmutableArray<Ast> EvalRaw(MethodDefinition method)
		{
			var actualMethod = method.UnwrapAsyncMethod();

			if (actualMethod.HasBody == false)
				return []; // this is an abstract method, nothing to evaluate

			var expressions = new List<Ast>();
			var methodBranches = CilMachine.Run(actualMethod);
			foreach (var mb in methodBranches)
			{
				expressions.AddRange(mb.Expressions);
				foreach (var ftn in mb.Expressions.OfFtn())
				{
					var lambdaBranches = EvalRaw(ftn.MethodImpl);
					expressions.AddRange(lambdaBranches);
				}
			}
			return [.. expressions];
		}

		public static ImmutableArray<Ast> Eval(AssemblyInfo asm, MethodDefinition method)
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
				var lambdaBranches = CilMachine.Run(lambdaMethod);
				foreach (var branch in lambdaBranches)
				{
					lambdaExpressions.AddRange(branch.Expressions);
					CollectLambdaExpressions(branch.Expressions, lambdaExpressions);
				}
			}
		}

		public static void PrettyPrintMethod(StringBuilder sb, MethodDefinition method, CilPoint pt)
		{
			sb.Append(method.DeclaringType.Namespace);
			sb.Append('.');
			sb.Append(method.DeclaringType.Name);
			sb.Append('.');
			sb.Append(method.Name);

			if (pt.SequencePoint != null)
			{
				sb.Append(" line ");
				sb.Append(pt.SequencePoint.StartLine);
			}
		}
		#endregion

		#region Implementation
		record CallChainNode(CallChainNode Parent, CallInfo Call);

		private static void PopulateCallChains(AssemblyInfo asm, MethodReference target, int level, CallInfo call, CallChainNode parent, HashSet<MethodReference> visitedMethods, List<List<CallInfo>> chains)
		{
			if (ReflectionExtensions.AreEqual(call.Method, target))
			{
				var chain = new List<CallInfo>(level + 1);
				for (var x = parent; x != null; x = x.Parent)
				{
					chain.Add(x.Call);
				}
				chain.Reverse();
				chains.Add(chain);
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

					if (asm.InterfaceImplementations.TryGetValue(innerCall.Method.DeclaringType, out var implTypes))
					{
						// this is an interface method, substitute it with implementations
						var implMethods = implTypes.SelectMany(x => x.Methods.Where(m => m.SignatureEquals(innerCall.Method)));
						foreach (var impl in implMethods)
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
		#endregion
	}
}
