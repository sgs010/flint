using System.Collections.Frozen;
using System.Collections.Immutable;
using Flint.Common;
using Mono.Cecil;

namespace Flint.Analyzers
{
	#region AssemblyDefinition
	sealed class AssemblyDefinition : Disposable
	{
		public required ModuleDefinition Module { get; init; }
		public required FrozenSet<TypeReference> EntityTypes { get; init; }
		public required FrozenDictionary<TypeReference, ImmutableArray<TypeReference>> InterfaceImplementations { get; init; }

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
		public static AssemblyDefinition Analyze(string path)
		{
			var module = ModuleDefinition.ReadModule(path, new ReaderParameters { ReadSymbols = true });
			var entityMap = new HashSet<TypeReference>();
			var interfaceMap = new Dictionary<TypeReference, List<TypeReference>>();

			foreach (var t in module.Types)
			{
				TryPopulateEntityMap(t, entityMap);
				TryPopulateInterfaceMap(t, interfaceMap);
			}

			return new AssemblyDefinition
			{
				Module = module,
				EntityTypes = entityMap.ToFrozenSet(),
				InterfaceImplementations = interfaceMap.ToFrozenDictionary(x => x.Key, x => x.Value.ToImmutableArray())
			};
		}
		#endregion

		#region Implementation
		private static void TryPopulateEntityMap(TypeDefinition type, HashSet<TypeReference> entityMap)
		{
			if (type.BaseType == null)
				return;
			if (type.BaseType.Namespace != "Microsoft.EntityFrameworkCore")
				return;
			if (type.BaseType.Name != "DbContext")
				return;

			foreach (var prop in type.Properties)
			{
				if (prop.PropertyType.Namespace != "Microsoft.EntityFrameworkCore")
					continue;
				if (prop.PropertyType.Name != "DbSet`1")
					continue;

				var entity = ((GenericInstanceType)prop.PropertyType).GenericArguments.First();
				entityMap.Add(entity);
			}
		}

		private static void TryPopulateInterfaceMap(TypeDefinition type, Dictionary<TypeReference, List<TypeReference>> interfaceMap)
		{
			if (type.IsInterface)
				return;

			foreach (var intr in type.Interfaces)
			{
				if (interfaceMap.TryGetValue(intr.InterfaceType, out var implementations) == false)
				{
					implementations = [];
					interfaceMap.Add(intr.InterfaceType, implementations);
				}
				implementations.Add(type);
			}
		}
		#endregion
	}
	#endregion
}
