using System.Collections.Frozen;
using System.Collections.Immutable;
using Flint.Common;
using Flint.Vm;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Analyzers
{
	#region CallInfo
	record CallInfo(MethodReference Method, SequencePoint SequencePoint);
	#endregion

	#region AssemblyDefinition
	sealed class AssemblyDefinition : Disposable
	{
		public required ModuleDefinition Module { get; init; }
		public required FrozenSet<TypeDefinition> EntityTypes { get; init; }
		public required FrozenDictionary<TypeReference, ImmutableArray<TypeDefinition>> InterfaceImplementations { get; init; }
		public required FrozenDictionary<MethodDefinition, ImmutableArray<Ast>> MethodExpressions { get; init; }
		public required FrozenDictionary<MethodReference, ImmutableArray<CallInfo>> MethodInnerCalls { get; init; }
		public required FrozenDictionary<MethodReference, ImmutableArray<CallInfo>> MethodOuterCalls { get; init; }

		protected override void BaseDispose(bool disposing)
		{
			if (disposing)
			{
				Module.Dispose();
			}
		}
	}
	#endregion

	#region AssemblyAnalyzer
	internal class AssemblyAnalyzer
	{
		#region Interface
		public static AssemblyDefinition Load(string path)
		{
			var module = ModuleDefinition.ReadModule(path, new ReaderParameters { ReadSymbols = true });
			var entityMap = new HashSet<TypeDefinition>();
			var interfaceMap = new Dictionary<TypeReference, List<TypeDefinition>>();
			var methodMap = new Dictionary<MethodDefinition, ImmutableArray<Ast>>(MethodReferenceEqualityComparer.Instance);

			foreach (var t in module.Types)
			{
				PopulateEntities(t, entityMap);
				PopulateInterfaces(t, interfaceMap);
				PopulateMethods(t, methodMap);
			}

			var innerCallMap = new Dictionary<MethodReference, List<CallInfo>>(MethodReferenceEqualityComparer.Instance);
			var outerCallMap = new Dictionary<MethodReference, List<CallInfo>>(MethodReferenceEqualityComparer.Instance);
			foreach (var m in methodMap)
			{
				PopulateCalls(m.Key, m.Value, innerCallMap, outerCallMap);
			}

			return new AssemblyDefinition
			{
				Module = module,
				EntityTypes = entityMap.ToFrozenSet(),
				InterfaceImplementations = interfaceMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray()),
				MethodExpressions = methodMap.ToFrozenDictionary(),
				MethodInnerCalls = innerCallMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray()),
				MethodOuterCalls = outerCallMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray())
			};
		}
		#endregion

		#region Implementation
		private static void PopulateEntities(TypeDefinition type, HashSet<TypeDefinition> entityMap)
		{
			if (type.BaseType == null)
				return;
			if (type.BaseType.Namespace != "Microsoft.EntityFrameworkCore")
				return;
			if (type.BaseType.Name != "DbContext")
				return;

			foreach (var prop in type.Properties)
			{
				if (prop.PropertyType.IsDbSet(out var entityType))
					entityMap.Add(entityType);
			}
		}

		private static void PopulateInterfaces(TypeDefinition type, Dictionary<TypeReference, List<TypeDefinition>> interfaceMap)
		{
			if (type.IsInterface)
				return;

			foreach (var intr in type.Interfaces)
			{
				var interfaceType = intr.InterfaceType;
				if (interfaceMap.TryGetValue(interfaceType, out var implementations) == false)
				{
					implementations = [];
					interfaceMap.Add(interfaceType, implementations);
				}
				implementations.Add(type);
			}
		}

		private static void PopulateMethods(TypeDefinition type, Dictionary<MethodDefinition, ImmutableArray<Ast>> methodMap)
		{
			if (type.IsInterface)
				return; // do not process interfaces
			if (type.IsCompilerGenerated())
				return; // do not process auto generated classes

			foreach (var method in type.Methods)
			{
				if (method.IsCompilerGenerated())
					continue; // do not process auto generated methods

				var expressions = MethodAnalyzer.EvalRaw(method);
				methodMap.Add(method, expressions);
			}
		}

		private static void PopulateCalls(MethodDefinition method, ImmutableArray<Ast> expressions, Dictionary<MethodReference, List<CallInfo>> innerCallMap, Dictionary<MethodReference, List<CallInfo>> outerCallMap)
		{
			var innerCalls = new List<CallInfo>();
			innerCallMap.Add(method, innerCalls);

			foreach (var call in expressions.OfCall().Distinct())
			{
				innerCalls.Add(new CallInfo(call.Method, call.SequencePoint));

				if (outerCallMap.TryGetValue(call.Method, out var outerCalls) == false)
				{
					outerCalls = [];
					outerCallMap.Add(call.Method, outerCalls);
				}
				outerCalls.Add(new CallInfo(method, call.SequencePoint));
			}
		}
		#endregion
	}
	#endregion
}
