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
		public required FrozenSet<TypeDefinition> EntityTypes { get; init; }
		public required FrozenDictionary<TypeDefinition, ImmutableArray<TypeDefinition>> InterfaceImplementations { get; init; }

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
		private static void TryPopulateEntityMap(TypeDefinition type, HashSet<TypeDefinition> entityMap)
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

				var entity = ((GenericInstanceType)prop.PropertyType).GenericArguments.First().Resolve();
				entityMap.Add(entity);
			}
		}

		private static void TryPopulateInterfaceMap(TypeDefinition type, Dictionary<TypeDefinition, List<TypeDefinition>> interfaceMap)
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
		#endregion
	}
	#endregion
}
