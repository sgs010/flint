using System.Collections.Frozen;
using System.Collections.Immutable;
using Flint.Common;
using Flint.Vm;
using Flint.Vm.Cil;
using Mono.Cecil;
using Cil = Flint.Vm.Cil;

namespace Flint.Analyzers
{
	#region CallInfo
	record CallInfo(MethodReference Method, CilPoint CilPoint);
	#endregion

	#region AssemblyInfo
	sealed class AssemblyInfo : Disposable
	{
		public required ModuleDefinition Module { get; init; }
		public required FrozenSet<TypeDefinition> EntityTypes { get; init; }
		public required FrozenSet<PropertyDefinition> EntityCollections { get; init; }
		public required FrozenSet<MethodReference> EntityGetSetMethods { get; init; }
		public required FrozenDictionary<TypeReference, ImmutableArray<TypeDefinition>> InterfaceImplementations { get; init; }
		public required FrozenDictionary<MethodDefinition, ImmutableArray<Ast>> MethodExpressions { get; init; }
		public required FrozenDictionary<MethodReference, ImmutableArray<CallInfo>> MethodInnerCalls { get; init; }
		public required FrozenDictionary<MethodReference, ImmutableArray<CallInfo>> MethodOuterCalls { get; init; }
		public required FrozenDictionary<MethodReference, string> MethodFullNames { get; init; }

		// roots are methods where IQueryable monad is unwrapped (ToListAsync and so on)
		public required FrozenSet<MethodReference> EFCoreRoots { get; init; }

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
		public static AssemblyInfo Load(IAnalyzerContext ctx, string path)
		{
			Stream dllStream = null, pdbStream = null;
			try
			{
				dllStream = File.OpenRead(path);

				var pdbPath = Path.ChangeExtension(path, ".pdb");
				if (File.Exists(pdbPath))
					pdbStream = File.OpenRead(pdbPath);

				return Load(ctx, dllStream, pdbStream);
			}
			finally
			{
				dllStream?.Dispose();
				pdbStream?.Dispose();
			}
		}

		public static AssemblyInfo Load(IAnalyzerContext ctx, Stream dllStream, Stream pdbStream)
		{
			var module = LoadModule(dllStream, pdbStream);

			var entityMap = new HashSet<TypeDefinition>();
			var entityPropMap = new HashSet<PropertyDefinition>();
			var entityGetSetMap = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance);
			var interfaceMap = new Dictionary<TypeReference, List<TypeDefinition>>();
			var methodMap = new Dictionary<MethodDefinition, ImmutableArray<Ast>>(MethodReferenceEqualityComparer.Instance);
			var methodNameMap = new Dictionary<MethodReference, string>(MethodReferenceEqualityComparer.Instance);

			foreach (var type in module.Types)
			{
				var tt = ctx.BeginTrace($"load type {type.FullName}");

				PopulateEntities(type, entityMap, entityPropMap, entityGetSetMap);
				PopulateInterfaces(type, interfaceMap);
				PopulateMethods(ctx, type, methodMap, methodNameMap);

				ctx.EndTrace(tt);
			}

			var innerCallMap = new Dictionary<MethodReference, HashSet<CallInfo>>(MethodReferenceEqualityComparer.Instance);
			var outerCallMap = new Dictionary<MethodReference, HashSet<CallInfo>>(MethodReferenceEqualityComparer.Instance);
			foreach (var m in methodMap)
			{
				PopulateCalls(m.Key, m.Value, innerCallMap, outerCallMap);
			}

			var efCoreRoots = outerCallMap.Keys
				.Where(x => x.HasFullName(EF_CORE_ROOTS))
				.ToFrozenSet(MethodReferenceEqualityComparer.Instance);

			return new AssemblyInfo
			{
				Module = module,
				EntityTypes = entityMap.ToFrozenSet(),
				EntityCollections = entityPropMap.ToFrozenSet(),
				EntityGetSetMethods = entityGetSetMap.ToFrozenSet(),
				InterfaceImplementations = interfaceMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray()),
				MethodExpressions = methodMap.ToFrozenDictionary(),
				MethodInnerCalls = innerCallMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray()),
				MethodOuterCalls = outerCallMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray()),
				MethodFullNames = methodNameMap.ToFrozenDictionary(),
				EFCoreRoots = efCoreRoots,
			};
		}

		public static string GetMethodFullName(AssemblyInfo asm, MethodReference method)
		{
			if (asm.MethodFullNames.TryGetValue(method, out var fullName))
				return fullName;
			return method.FullName;
		}
		#endregion

		#region Implementation
		sealed class AssemblyResolver : DefaultAssemblyResolver
		{
			public override Mono.Cecil.AssemblyDefinition Resolve(AssemblyNameReference name)
			{
				try
				{
					return base.Resolve(name);
				}
				catch (Mono.Cecil.AssemblyResolutionException)
				{
					return null;
				}
			}
		}

		sealed class CallCmp : IEqualityComparer<Cil.Call>
		{
			public static CallCmp Instance = new();

			public bool Equals(Call x, Call y)
			{
				return x.CilPoint.Equals(y.CilPoint);
			}

			public int GetHashCode(Call obj)
			{
				return obj.CilPoint.GetHashCode();
			}
		}

		private static readonly FrozenSet<string> EF_CORE_ROOTS = [
			"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AsAsyncEnumerable",
			"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToArrayAsync",
			"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToDictionaryAsync",
			"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToHashSetAsync",
			"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync",
			"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstAsync",
			"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync",
			"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.LastAsync",
			"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.LastOrDefaultAsync",
			"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleAsync",
			"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleOrDefaultAsync",
		];

		private static ModuleDefinition LoadModule(Stream dllStream, Stream pdbStream)
		{
			var parameters = new ReaderParameters { AssemblyResolver = new AssemblyResolver() };
			if (pdbStream != null && pdbStream.Length > 0)
			{
				parameters.ReadSymbols = true;
				parameters.SymbolStream = pdbStream;
			}

			// load module with pdb
			try
			{
				return ModuleDefinition.ReadModule(dllStream, parameters);
			}
			catch (Mono.Cecil.Cil.SymbolsNotFoundException) { }
			catch (Mono.Cecil.Cil.SymbolsNotMatchingException) { }

			// if we a here then pdb is invalid, so ignore it and try to load module again
			parameters.ReadSymbols = false;
			parameters.SymbolStream = null;
			dllStream.Position = 0;
			return ModuleDefinition.ReadModule(dllStream, parameters);
		}

		private static void PopulateEntities(TypeDefinition type, HashSet<TypeDefinition> entityMap, HashSet<PropertyDefinition> entityPropMap, HashSet<MethodReference> entityGetSetMap)
		{
			if (type.BaseType == null)
				return;
			if (type.BaseType.Namespace != "Microsoft.EntityFrameworkCore")
				return;
			if (type.BaseType.Name != "DbContext")
				return;

			foreach (var prop in type.Properties)
			{
				if (prop.PropertyType.IsDbSet(out var entityType) == false)
					continue;

				entityMap.Add(entityType);
				entityPropMap.Add(prop);

				foreach (var ep in entityType.Properties)
				{
					if (ep.GetMethod != null)
						entityGetSetMap.Add(ep.GetMethod);
					if (ep.SetMethod != null)
						entityGetSetMap.Add(ep.SetMethod);
				}
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

		private static void PopulateMethods(IAnalyzerContext ctx, TypeDefinition type, Dictionary<MethodDefinition, ImmutableArray<Ast>> methodMap, Dictionary<MethodReference, string> methodNameMap)
		{
			if (type.IsInterface)
				return; // do not process interfaces
			if (type.IsCompilerGenerated())
				return; // do not process auto generated classes

			foreach (var method in type.Methods)
			{
				methodNameMap.Add(method, method.FullName);

				if (method.IsCompilerGenerated())
					continue; // do not process auto generated methods

				var tt = ctx.BeginTrace($"load method {method.Name}");

				var expressions = MethodAnalyzer.EvalRaw(method);
				methodMap.Add(method, [.. expressions.Distinct()]);

				ctx.EndTrace(tt);
			}
		}

		private static void PopulateCalls(MethodDefinition method, ImmutableArray<Ast> expressions, Dictionary<MethodReference, HashSet<CallInfo>> innerCallMap, Dictionary<MethodReference, HashSet<CallInfo>> outerCallMap)
		{
			var innerCalls = new HashSet<CallInfo>();
			innerCallMap.Add(method, innerCalls);

			foreach (var call in expressions.OfCall().Distinct(CallCmp.Instance))
			{
				innerCalls.Add(new CallInfo(call.Method, call.CilPoint));

				if (outerCallMap.TryGetValue(call.Method, out var outerCalls) == false)
				{
					outerCalls = [];
					outerCallMap.Add(call.Method, outerCalls);
				}
				outerCalls.Add(new CallInfo(method, call.CilPoint));
			}
		}
		#endregion
	}
	#endregion
}
