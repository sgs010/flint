using System.Collections.Frozen;
using System.Collections.Immutable;
using Flint.Common;
using Flint.Vm;
using Mono.Cecil;

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
		public required Dictionary<TypeReference, string> TypeFullNameIndex { get; init; }
		public required Dictionary<MethodReference, string> MethodFullNameIndex { get; init; }
		public required Dictionary<MethodReference, string> MethodLongNameIndex { get; init; }
		public required Dictionary<MethodDefinition, ImmutableArray<Ast>> MethodExpressions { get; init; }
		public required Dictionary<MethodReference, bool> MethodEFCoreRootIndex { get; init; }

		// methods called in a key method
		public required FrozenDictionary<MethodReference, ImmutableArray<CallInfo>> MethodInnerCalls { get; init; }

		// methrods where key method is called
		public required FrozenDictionary<MethodReference, ImmutableArray<CallInfo>> MethodOuterCalls { get; init; }

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
		public static AssemblyInfo Load(string path)
		{
			Stream dllStream = null, pdbStream = null;
			try
			{
				dllStream = File.OpenRead(path);

				var pdbPath = Path.ChangeExtension(path, ".pdb");
				if (File.Exists(pdbPath))
					pdbStream = File.OpenRead(pdbPath);

				return Load(dllStream, pdbStream);
			}
			finally
			{
				dllStream?.Dispose();
				pdbStream?.Dispose();
			}
		}

		public static AssemblyInfo Load(Stream dllStream, Stream pdbStream)
		{
			var module = LoadModule(dllStream, pdbStream);

			var entityMap = new HashSet<TypeDefinition>(TypeReferenceEqualityComparer.Instance);
			var entityPropMap = new HashSet<PropertyDefinition>(PropertyReferenceEqualityComparer.Instance);
			var entityGetSetMap = new HashSet<MethodReference>(MethodReferenceEqualityComparer.Instance);
			var interfaceMap = new Dictionary<TypeReference, List<TypeDefinition>>(TypeReferenceEqualityComparer.Instance);
			var methodMap = new HashSet<MethodDefinition>(MethodReferenceEqualityComparer.Instance);

			foreach (var type in module.Types)
			{
				PopulateEntities(type, entityMap, entityPropMap, entityGetSetMap);
				PopulateInterfaces(type, interfaceMap);
				PopulateMethods(type, methodMap);
			}

			var innerCallMap = new Dictionary<MethodReference, HashSet<CallInfo>>(MethodReferenceEqualityComparer.Instance);
			var outerCallMap = new Dictionary<MethodReference, HashSet<CallInfo>>(MethodReferenceEqualityComparer.Instance);
			foreach (var m in methodMap)
			{
				PopulateCalls(m, innerCallMap, outerCallMap);
			}

			var typeFullNameIndex = new Dictionary<TypeReference, string>(TypeReferenceEqualityComparer.Instance);
			var methodFullNameIndex = new Dictionary<MethodReference, string>(MethodReferenceEqualityComparer.Instance);
			var methodLongNameIndex = new Dictionary<MethodReference, string>(MethodReferenceEqualityComparer.Instance);

			var efCoreRoots = outerCallMap.Keys
				.Where(x => MethodHasLongName(typeFullNameIndex, methodLongNameIndex, x, EF_CORE_ROOTS))
				.ToFrozenSet(MethodReferenceEqualityComparer.Instance);

			return new AssemblyInfo
			{
				Module = module,
				EntityTypes = entityMap.ToFrozenSet(TypeDefinitionEqualityComparer.Instance),
				EntityCollections = entityPropMap.ToFrozenSet(PropertyDefinitionEqualityComparer.Instance),
				EntityGetSetMethods = entityGetSetMap.ToFrozenSet(MethodReferenceEqualityComparer.Instance),
				InterfaceImplementations = interfaceMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray(), TypeReferenceEqualityComparer.Instance),
				MethodInnerCalls = innerCallMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray(), MethodReferenceEqualityComparer.Instance),
				MethodOuterCalls = outerCallMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray(), MethodReferenceEqualityComparer.Instance),
				EFCoreRoots = efCoreRoots,
				TypeFullNameIndex = typeFullNameIndex,
				MethodFullNameIndex = methodFullNameIndex,
				MethodLongNameIndex = methodLongNameIndex,
				MethodExpressions = new(MethodReferenceEqualityComparer.Instance),
				MethodEFCoreRootIndex = new(MethodReferenceEqualityComparer.Instance),
			};
		}

		public static string GetTypeFullName(AssemblyInfo asm, TypeReference type)
		{
			return GetTypeFullName(asm.TypeFullNameIndex, type);
		}

		public static string GetMethodFullName(AssemblyInfo asm, MethodReference method)
		{
			return GetMethodFullName(asm.MethodFullNameIndex, method);
		}

		public static bool MethodHasLongName(AssemblyInfo asm, MethodReference method, string name)
		{
			return MethodHasLongName(asm.TypeFullNameIndex, asm.MethodLongNameIndex, method, name);
		}

		public static bool MethodHasEFCoreRoots(AssemblyInfo asm, MethodReference method)
		{
			if (asm.MethodEFCoreRootIndex.TryGetValue(method, out var hasRoots) == false)
			{
				var roots = MethodAnalyzer.GetCallChains(asm, method, asm.EFCoreRoots);
				hasRoots = (roots.Count > 0);
				asm.MethodEFCoreRootIndex.Add(method, hasRoots);
			}
			return hasRoots;
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

		private static TypeReference COMPILER_GENERATED_ATTRIBUTE_TYPE;

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

		private static string GetTypeFullName(Dictionary<TypeReference, string> typeMap, TypeReference type)
		{
			if (typeMap.TryGetValue(type, out var fullName) == false)
			{
				fullName = type.FullName;
				typeMap.Add(type, fullName);
			}
			return fullName;
		}

		private static string GetMethodFullName(Dictionary<MethodReference, string> methodMap, MethodReference method)
		{
			if (methodMap.TryGetValue(method, out var fullName) == false)
			{
				fullName = method.FullName;
				methodMap.Add(method, fullName);
			}
			return fullName;
		}

		private static string GetMethodLongName(Dictionary<TypeReference, string> typeMap, Dictionary<MethodReference, string> methodMap, MethodReference method)
		{
			if (methodMap.TryGetValue(method, out var longName) == false)
			{
				longName = GetTypeFullName(typeMap, method.DeclaringType) + "." + method.Name;
				methodMap.Add(method, longName);
			}
			return longName;
		}

		private static bool MethodHasLongName(Dictionary<TypeReference, string> typeMap, Dictionary<MethodReference, string> methodMap, MethodReference method, string longName)
		{
			var methodLongName = GetMethodLongName(typeMap, methodMap, method);
			return methodLongName.Equals(longName, StringComparison.Ordinal);
		}

		private static bool MethodHasLongName(Dictionary<TypeReference, string> typeMap, Dictionary<MethodReference, string> methodMap, MethodReference method, IReadOnlySet<string> names)
		{
			var methodLongName = GetMethodLongName(typeMap, methodMap, method);
			return names.Contains(methodLongName);
		}

		private static bool IsCompilerGenerated(IEnumerable<CustomAttribute> attributes)
		{
			if (COMPILER_GENERATED_ATTRIBUTE_TYPE != null)
				return attributes.Any(x => Are.Equal(x.AttributeType, COMPILER_GENERATED_ATTRIBUTE_TYPE));

			var attr = attributes
				.Select(x => x.AttributeType)
				.FirstOrDefault(x => x.Namespace == "System.Runtime.CompilerServices" && x.Name == "CompilerGeneratedAttribute");
			if (attr == null)
				return false;

			COMPILER_GENERATED_ATTRIBUTE_TYPE = attr;
			return true;
		}

		private static bool IsCompilerGenerated(TypeDefinition type)
		{
			return IsCompilerGenerated(type.CustomAttributes);
		}

		public static bool IsCompilerGenerated(MethodDefinition method)
		{
			return IsCompilerGenerated(method.CustomAttributes);
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

		private static void PopulateMethods(TypeDefinition type, HashSet<MethodDefinition> methods)
		{
			if (type.IsInterface)
				return; // do not process interfaces
			if (IsCompilerGenerated(type))
				return; // do not process auto generated classes

			foreach (var method in type.Methods)
			{
				if (IsCompilerGenerated(method))
					continue; // do not process auto generated methods
				methods.Add(method);
			}
		}

		private static void PopulateCalls(MethodDefinition method, Dictionary<MethodReference, HashSet<CallInfo>> innerCallMap, Dictionary<MethodReference, HashSet<CallInfo>> outerCallMap)
		{
			if (method.Name == "NoOutbox") { }

			if (innerCallMap.TryGetValue(method, out var innerCalls) == false)
			{
				innerCalls = [];
				innerCallMap.Add(method, innerCalls);
			}

			foreach (var (call, pt) in MethodAnalyzer.GetCalls(method))
			{
				innerCalls.Add(new CallInfo(call, pt));

				if (outerCallMap.TryGetValue(call, out var outerCalls) == false)
				{
					outerCalls = [];
					outerCallMap.Add(call, outerCalls);
				}
				outerCalls.Add(new CallInfo(method, pt));
			}
		}
		#endregion
	}
	#endregion
}
