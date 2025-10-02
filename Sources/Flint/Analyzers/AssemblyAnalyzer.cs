using System.Collections.Frozen;
using System.Collections.Immutable;
using Flint.Common;
using Flint.Vm;
using Mono.Cecil;

namespace Flint.Analyzers
{
	#region AssemblyDefinition
	sealed class AssemblyDefinition : Disposable
	{
		public required ModuleDefinition Module { get; init; }
		public required FrozenSet<TypeDefinition> EntityTypes { get; init; }
		public required FrozenDictionary<TypeDefinition, ImmutableArray<TypeDefinition>> InterfaceImplementations { get; init; }
		public required FrozenDictionary<MethodDefinition, ImmutableArray<Ast>> MethodExpressions { get; init; }

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
			var interfaceMap = new Dictionary<TypeDefinition, List<TypeDefinition>>();
			var methodMap = new Dictionary<MethodDefinition, List<Ast>>();

			foreach (var t in module.Types)
			{
				TryPopulateEntities(t, entityMap);
				TryPopulateInterfaces(t, interfaceMap);
				TryPopulateMethods(t, methodMap);
			}

			return new AssemblyDefinition
			{
				Module = module,
				EntityTypes = entityMap.ToFrozenSet(),
				InterfaceImplementations = interfaceMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray()),
				MethodExpressions = methodMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray())
			};
		}
		#endregion

		#region Implementation
		private static void TryPopulateEntities(TypeDefinition type, HashSet<TypeDefinition> entityMap)
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

		private static void TryPopulateInterfaces(TypeDefinition type, Dictionary<TypeDefinition, List<TypeDefinition>> interfaceMap)
		{
			if (type.IsInterface)
				return;

			foreach (var intr in type.Interfaces)
			{
				var interfaceType = intr.InterfaceType.Resolve();
				if (interfaceMap.TryGetValue(interfaceType, out var implementations) == false)
				{
					implementations = [];
					interfaceMap.Add(interfaceType, implementations);
				}
				implementations.Add(type);
			}
		}

		private static void TryPopulateMethods(TypeDefinition type, Dictionary<MethodDefinition, List<Ast>> methodMap)
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
		#endregion
	}
	#endregion
}
