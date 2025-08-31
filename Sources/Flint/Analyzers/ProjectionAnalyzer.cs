using System.Text;
using Flint.Common;
using Flint.Vm;
using Flint.Vm.Match;
using Mono.Cecil;
using Cil = Flint.Vm.Cil;
using Match = Flint.Vm.Match;

namespace Flint.Analyzers
{
	internal class ProjectionAnalyzer
	{
		#region Interface
		public static void Run(IAnalyzerContext ctx, ModuleDefinition asm, string className = null, string methodName = null)
		{
			var entityTypes = GetEntityTypes(asm);
			foreach (var mtd in GetMethods(asm, className, methodName))
			{
				Analyze(ctx, mtd, entityTypes);
			}
		}
		#endregion

		#region Implementation
		sealed class EntityDefinition
		{
			public TypeDefinition Type { get; init; }
			public Dictionary<PropertyReference, EntityDefinition> Properties { get; } = [];
		}

		private static HashSet<TypeReference> GetEntityTypes(ModuleDefinition asm)
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

		private static IEnumerable<MethodDefinition> GetMethods(ModuleDefinition asm, string className = null, string methodName = null)
		{
			foreach (var type in asm.Types)
			{
				if (className != null && type.Name != className)
					continue;

				foreach (var mtd in type.Methods)
				{
					if (methodName != null && mtd.Name != methodName)
						continue;

					yield return mtd;
				}
			}
		}

		private static void Analyze(IAnalyzerContext ctx, MethodDefinition mtd, HashSet<TypeReference> entityTypes)
		{
			// eval method body
			var expressions = CilMachine.Run(mtd);

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

				// some methods (i.e. ToDictionaryAsync) use lambdas
				// we must analyze such lambdas too because they can read entity properties
				var lambdas = CaptureLambdas(root,
				[
					"Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToDictionaryAsync",
				]);
				rootExpressions.AddRange(lambdas);
			}

			// gather accessed properties
			var entities = new List<EntityDefinition>();
			foreach (var root in roots)
			{
				if (marks.TryGetValue(root, out var rootExpressions) == false)
					continue;

				// et is T from METHOD<T> (i.e. ToListAsync<T>)
				var et = (TypeDefinition)((GenericInstanceMethod)root.Method).GenericArguments.First();
				if (entityTypes.Contains(et) == false)
					continue;

				var entity = CreateEntityDefinition(et, rootExpressions, entityTypes);
				if (entity.Properties.Count == 0)
					continue;

				entities.Add(entity);
			}

			// report issues
			foreach (var entity in entities)
			{
				if (AllProperiesAreAccessed(entity))
					continue; // do not advise a projection if all properties are accessed

				var sb = new StringBuilder();
				sb.Append("consider using projection { ");
				PrettyPrintEntity(sb, entity, null);
				sb.Append(" } in method ");
				PrettyPrintMethod(sb, mtd);
				ctx.Log(sb.ToString());
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

		private static List<Ast> CaptureLambdas(Cil.Call call, IEnumerable<string> methodNames)
		{
			// if method uses lambdas (i.e. ToDictionaryAsync(x => x.Id) and so on)
			// we eval a body of such lambda and return the result expressions
			// later we gonna use these expressions to look for entity property access operations

			var lambdas = new List<Ast>();
			var callName = call.Method.DeclaringType.FullName + "." + call.Method.Name;
			foreach (var methodName in methodNames)
			{
				if (callName != methodName)
					continue;

				var (captures, ok) = call.Match(
					new Match.Func(), // Func is ldftn IL instruction, lambdas are translated into this
					true);
				if (ok == false)
					continue;

				foreach (Cil.Func func in captures.Values)
				{
					var expressions = CilMachine.Run(func.Method);
					lambdas.AddRange(expressions);
				}
			}
			return lambdas;
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

		private static EntityDefinition CreateEntityDefinition(TypeDefinition type, IReadOnlyCollection<Ast> expressions, HashSet<TypeReference> entityTypes)
		{
			var entity = new EntityDefinition { Type = type };
			foreach (var prop in entity.Type.Properties)
			{
				foreach (var expr in expressions)
				{
					var (captures, ok) = expr.Match(
						new Match.Call(Any.Instance, prop.GetMethod.FullName, Any.Args));
					if (ok == false)
						continue;

					if (entity.Properties.ContainsKey(prop))
						continue;

					EntityDefinition child = null;
					if (IsGenericCollection(prop, out var itemType, entityTypes))
						child = CreateEntityDefinition(itemType.Resolve(), expressions, entityTypes);
					else if (entityTypes.Contains(prop.PropertyType))
						child = CreateEntityDefinition(prop.PropertyType.Resolve(), expressions, entityTypes);

					entity.Properties.Add(prop, child);
				}
			}
			return entity;
		}

		private static bool IsGenericCollection(PropertyReference prop, out TypeReference itemType, HashSet<TypeReference> allowedTypes = null)
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

		private static bool AllProperiesAreAccessed(EntityDefinition entity)
		{
			if (entity.Type.Properties.Count != entity.Properties.Count)
				return false;

			foreach (var child in entity.Properties.Values)
			{
				if (child == null)
					continue;
				if (AllProperiesAreAccessed(child) == false)
					return false;
			}

			return true;
		}

		private static void PrettyPrintMethod(StringBuilder sb, MethodDefinition mtd)
		{
			sb.Append(mtd.DeclaringType.Namespace);
			sb.Append('.');
			sb.Append(mtd.DeclaringType.Name);
			sb.Append('.');
			sb.Append(mtd.Name);
			sb.Append("()");
		}

		private static void PrettyPrintEntity(StringBuilder sb, EntityDefinition entity, string prefix)
		{
			var needSeparator = false;
			foreach (var x in entity.Properties)
			{
				if (needSeparator)
					sb.Append(", ");
				needSeparator = true;

				if (prefix != null)
					sb.Append(prefix);

				var prop = x.Key;
				var value = x.Value;
				if (value == null)
				{
					// property is a simple value (int, string and so on)
					sb.Append(prop.Name);
				}
				else
				{
					if (IsGenericCollection(prop, out var _))
					{
						// property is a collection of ONE-TO-MANY entities
						sb.Append(prop.Name);
						sb.Append(" = { ");
						PrettyPrintEntity(sb, value, null);
						sb.Append(" }");
					}
					else
					{
						// property is a chained ONE-TO-ONE entity
						PrettyPrintEntity(sb, value, prop.Name + ".");
					}
				}
			}
		}
		#endregion
	}
}
