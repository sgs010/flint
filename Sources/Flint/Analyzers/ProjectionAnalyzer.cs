using System.Text;
using Flint.Common;
using Flint.Vm;
using Flint.Vm.Match;
using Mono.Cecil;
using Mono.Cecil.Cil;
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
				// analyze
				var actualMethod = UnwrapAsyncMethod(mtd);
				var projections = Analyze(actualMethod, entityTypes);

				// report issues
				foreach (var p in projections)
				{
					var sb = new StringBuilder();
					sb.Append("consider using projection { ");
					PrettyPrintEntity(sb, p.Entity, null);
					sb.Append(" } in method ");
					PrettyPrintMethod(sb, mtd, p.SequencePoint);
					ctx.Log(sb.ToString());
				}
			}
		}
		#endregion

		#region Implementation
		sealed class EntityDefinition
		{
			public TypeDefinition Type { get; init; }
			public Dictionary<PropertyReference, EntityDefinition> Properties { get; } = [];
			public bool IsChanged { get; set; }
		}

		sealed class ProjectionDefinition
		{
			public EntityDefinition Entity { get; init; }
			public SequencePoint SequencePoint { get; init; }
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

		private static MethodDefinition UnwrapAsyncMethod(MethodDefinition method)
		{
			// check if method is async and return actual implementation
			MethodDefinition asyncMethod = null;
			if (method.HasCustomAttributes)
			{
				var asyncAttr = method.CustomAttributes.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.AsyncStateMachineAttribute");
				if (asyncAttr != null)
				{
					var stmType = (TypeDefinition)asyncAttr.ConstructorArguments[0].Value;
					asyncMethod = stmType.Methods.First(x => x.Name == "MoveNext");
				}
			}
			return asyncMethod ?? method;
		}

		private static List<ProjectionDefinition> Analyze(MethodDefinition mtd, HashSet<TypeReference> entityTypes)
		{
			// eval method body
			var expressions = new List<Ast>();
			Eval(mtd, expressions);

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
				CollectLambdaExpressions(rootExpressions, lambdas);
				rootExpressions.AddRange(lambdas);
			}

			// gather accessed properties
			var projections = new List<ProjectionDefinition>();
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
					continue; // no properties accessed
				if (AllProperiesAreAccessed(entity))
					continue; // do not advise a projection if all properties are accessed
				if (SomePropertiesAreChanged(entity))
					continue; // do not advise a projection if entity is changed

				projections.Add(new ProjectionDefinition { Entity = entity, SequencePoint = root.SequencePoint });
			}
			return projections;
		}

		private static void Eval(MethodDefinition mtd, List<Ast> expressions)
		{
			// eval method body
			var methodExpressions = CilMachine.Run(mtd);
			expressions.AddRange(methodExpressions);

			// eval lambdas
			foreach (var expr in methodExpressions)
			{
				var (captures, ok) = expr.Match(
					new Match.Ftn(),
					true);
				if (ok == false)
					continue;

				foreach (Cil.Ftn ftn in captures.Values)
				{
					var lambdaMethod = UnwrapAsyncMethod(ftn.Method);
					Eval(lambdaMethod, expressions);
				}
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

		private static void CollectLambdaExpressions(IEnumerable<Ast> methodExpressions, List<Ast> lambdaExpressions)
		{
			foreach (var expr in methodExpressions)
			{
				var (captures, ok) = expr.Match(
					new Match.Ftn(), // Ftn is ldftn IL instruction, lambdas are translated into this
					true);
				if (ok == false)
					continue;

				foreach (Cil.Ftn ftn in captures.Values)
				{
					var lambdaMethod = UnwrapAsyncMethod(ftn.Method);
					var lambda = CilMachine.Run(lambdaMethod);
					lambdaExpressions.AddRange(lambda);
					CollectLambdaExpressions(lambda, lambdaExpressions);
				}
			}
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
					// check write (call of set_Property method)
					var (captures, ok) = expr.Match(
						new Match.Call(Any.Instance, prop.SetMethod.FullName, Any.Args),
						true);
					if (ok)
						entity.IsChanged = true;

					// check read (call of get_Property method)
					(captures, ok) = expr.Match(
						new Match.Call(Any.Instance, prop.GetMethod.FullName, Any.Args),
						true);
					if (ok == false)
						continue; // prop is not accessed, nothing to project

					if (entity.Properties.ContainsKey(prop))
						continue; // this prop is already marked, no need to process it again

					EntityDefinition child = null;
					if (IsGenericCollection(prop, out var itemType, entityTypes))
					{
						child = CreateEntityDefinition(itemType.Resolve(), expressions, entityTypes);
						if (entity.IsChanged == false)
							entity.IsChanged = IsCollectionChanged(expr, captures.Values);
					}
					else if (entityTypes.Contains(prop.PropertyType))
					{
						child = CreateEntityDefinition(prop.PropertyType.Resolve(), expressions, entityTypes);
					}
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

		private static bool SomePropertiesAreChanged(EntityDefinition entity)
		{
			if (entity.IsChanged)
				return true;

			foreach (var child in entity.Properties.Values)
			{
				if (child == null)
					continue;
				if (SomePropertiesAreChanged(child))
					return true;
			}

			return false;
		}

		private static void PrettyPrintMethod(StringBuilder sb, MethodDefinition mtd, SequencePoint sp)
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
