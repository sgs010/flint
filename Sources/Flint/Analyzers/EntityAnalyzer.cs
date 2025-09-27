using System.Text;
using Flint.Common;
using Flint.Vm;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Cil = Flint.Vm.Cil;
using Match = Flint.Vm.Match;

namespace Flint.Analyzers
{
	#region EntityDefinition
	class EntityDefinition
	{
		public required MethodDefinition Method { get; init; }
		public required Ast Root { get; init; }
		public required TypeDefinition Type { get; init; }
		public required EntityPropertyDefinition[] Properties { get; init; }
	}
	#endregion

	#region EntityPropertyDefinition
	class EntityPropertyDefinition
	{
		public required PropertyReference Property { get; init; }
		public required EntityDefinition Entity { get; init; }
		public required bool Read { get; init; }
		public required bool Write { get; init; }
	}
	#endregion

	#region EntityAnalyzer
	internal class EntityAnalyzer
	{
		#region Interface
		public static bool SomePropertiesAreChanged(EntityDefinition entity)
		{
			foreach (var prop in entity.Properties)
			{
				if (prop.Write)
					return true;
				if (prop.Entity != null && SomePropertiesAreChanged(prop.Entity))
					return true;
			}
			return false;
		}

		public static bool IsGenericCollection(PropertyReference prop, out TypeReference itemType, HashSet<TypeReference> allowedTypes = null)
		{
			// check if property is a System.Collections.Generic.ICollection<T>

			itemType = null;

			if (prop.PropertyType.IsGenericInstance == false)
				return false;
			if (prop.PropertyType.Namespace != "System.Collections.Generic")
				return false;
			if (prop.PropertyType.Name != "ICollection`1")
				return false;

			// get T from System.Collections.Generic.ICollection<T>
			var t = ((GenericInstanceType)prop.PropertyType).GenericArguments.First();
			if (allowedTypes != null && allowedTypes.Contains(t) == false)
				return false;

			itemType = t;
			return true;
		}

		public static HashSet<TypeReference> GetEntityTypes(ModuleDefinition asm)
		{
			// look for classes inherited from Microsoft.EntityFrameworkCore.DbContext
			// browse it's properties and collect T from Microsoft.EntityFrameworkCore.DbSet<T>

			var entityTypes = new HashSet<TypeReference>();
			foreach (var type in asm.Types)
			{
				if (type.BaseType == null)
					continue;
				if (type.BaseType.Namespace != "Microsoft.EntityFrameworkCore")
					continue;
				if (type.BaseType.Name != "DbContext")
					continue;

				foreach (var prop in type.Properties)
				{
					if (prop.PropertyType.Namespace != "Microsoft.EntityFrameworkCore")
						continue;
					if (prop.PropertyType.Name != "DbSet`1")
						continue;

					var entity = ((GenericInstanceType)prop.PropertyType).GenericArguments.First();
					entityTypes.Add(entity);
				}
			}
			return entityTypes;
		}

		public static EntityDefinition[] Analyze(ModuleDefinition asm, HashSet<TypeReference> entityTypes, string className = null, string methodName = null)
		{
			var entities = new List<EntityDefinition>();
			foreach (var method in MethodAnalyzer.GetMethods(asm, className, methodName))
			{
				Analyze(method, entityTypes, entities);
			}
			return entities.ToArray();
		}

		public static void PrettyPrintMethod(StringBuilder sb, MethodDefinition mtd, SequencePoint sp)
		{
			sb.Append(mtd.DeclaringType.Namespace);
			sb.Append('.');
			sb.Append(mtd.DeclaringType.Name);
			sb.Append('.');
			sb.Append(mtd.Name);

			if (sp != null)
			{
				sb.Append(" line ");
				sb.Append(sp.StartLine);
			}
		}
		#endregion

		#region Implementation
		private static void Analyze(MethodDefinition method, HashSet<TypeReference> entityTypes, List<EntityDefinition> entities)
		{
			// eval method body
			var expressions = MethodAnalyzer.Eval(method);

			// find roots (methods where IQueryable monad is unwrapped; ToListAsync and so on)
			// for every found root mark every ast accessible from it
			var roots = new HashSet<Cil.Call>();
			var marks = new Dictionary<Ast, List<Ast>>();
			foreach (var expr in expressions)
			{
				var (root, ok) = CaptureAnyRoot(expr,
				[
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
				]);
				if (ok == false)
					continue;

				roots.Add(root);
				var rootExpressions = marks.GetOrAddValue(root);
				Mark(expr, root, rootExpressions);

				// some methods (i.e. ToDictionaryAsync) use lambdas, analyze them too
				var lambdas = new List<Ast>();
				MethodAnalyzer.CollectLambdaExpressions(rootExpressions, lambdas);
				rootExpressions.AddRange(lambdas);
			}

			// gather accessed properties
			foreach (var root in roots)
			{
				if (marks.TryGetValue(root, out var rootExpressions) == false)
					continue;

				// et is T from METHOD<T> (i.e. ToListAsync<T>)
				var et = (TypeDefinition)((GenericInstanceMethod)root.Method).GenericArguments.First();
				if (entityTypes.Contains(et) == false)
					continue;

				var entity = CreateEntityDefinition(method, root, et, rootExpressions, entityTypes);
				entities.Add(entity);
			}
		}

		private static (Cil.Call root, bool ok) CaptureAnyRoot(Ast expression, IEnumerable<string> methodNames)
		{
			foreach (var name in methodNames)
			{
				var (captures, ok) = expression.Match(
					new Match.Call(null, name, Match.Any.Args),
					true);
				if (ok == false)
					continue;

				var root = (Cil.Call)captures.Values.First();
				return (root, true);
			}
			return (null, false);
		}

		private static void Mark(Ast expression, Ast root, List<Ast> marks)
		{
			// traverse expression tree top down and mark every node untill we reach root node

			if (expression == null)
				return;
			if (expression == root)
				return;

			marks.Add(expression);
			foreach (var child in expression.GetChildren())
				Mark(child, root, marks);
		}

		private static EntityDefinition CreateEntityDefinition(MethodDefinition mtd, Ast root, TypeDefinition type, IReadOnlyCollection<Ast> expressions, HashSet<TypeReference> entityTypes)
		{
			var entityProperties = new List<EntityPropertyDefinition>(type.Properties.Count);
			foreach (var prop in type.Properties)
			{
				bool propRead = false, propWrite = false;
				EntityDefinition propEnt = null;
				foreach (var expr in expressions)
				{
					// check read (call of get_Property method)
					var (captureRead, ok) = expr.Match(
						new Match.Call(Match.Any.Instance, prop.GetMethod.FullName, Match.Any.Args),
						true);
					if (ok)
						propRead = true;

					// check write (call of set_Property method)
					(_, ok) = expr.Match(
						new Match.Call(Match.Any.Instance, prop.SetMethod.FullName, Match.Any.Args),
						true);
					if (ok)
						propWrite = true;

					if (propRead == false && propWrite == false)
						continue;

					if (propEnt == null)
					{
						if (IsGenericCollection(prop, out var itemType, entityTypes))
						{
							propEnt = CreateEntityDefinition(mtd, root, itemType.Resolve(), expressions, entityTypes);
							propWrite = IsCollectionChanged(expr, captureRead.Values);
						}
						else if (entityTypes.Contains(prop.PropertyType))
						{
							propEnt = CreateEntityDefinition(mtd, root, prop.PropertyType.Resolve(), expressions, entityTypes);
						}
					}
				}
				if (propRead || propWrite)
					entityProperties.Add(new EntityPropertyDefinition { Property = prop, Entity = propEnt, Read = propRead, Write = propWrite });
			}
			return new EntityDefinition { Method = mtd, Root = root, Type = type, Properties = entityProperties.ToArray() };
		}

		private static bool IsCollectionChanged(Ast expr, IReadOnlyCollection<Ast> captures)
		{
			string[] methods = ["Add", "Remove"];
			foreach (var mtd in methods)
			{
				foreach (var capture in captures)
				{
					var (_, ok) = expr.Match(
						new Match.Call(capture, mtd, Match.Any.Args),
						true);
					if (ok)
						return true;
				}
			}
			return false;
		}
		#endregion
	}
	#endregion
}
