using Flint.Common;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm
{
	static class EvalMachine
	{
		#region Interface
		public static List<Ast> Run(MethodDefinition method)
		{
			// check if method is async and resolve actual implementation
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

			var ctx = new RoutineContext(asyncMethod ?? method);
			Run(ctx);
			return ctx.Expressions.ToList();
		}
		#endregion

		#region Implementation
		class RoutineContext
		{
			public readonly MethodDefinition Method;
			public readonly Dictionary<Tuple<Ast, FieldReference>, Ast> Memory;
			public readonly Ast[] Variables;
			public readonly Stack<Ast> Stack;
			public readonly HashSet<Ast> Expressions;
			public RoutineContext(MethodDefinition method)
			{
				Method = method;
				Memory = [];
				Variables = new Ast[method.Body.Variables.Count];
				Stack = new Stack<Ast>(method.Body.MaxStackSize);
				Expressions = [];
			}
		}

		private static void Run(RoutineContext ctx)
		{
			foreach (var instruction in ctx.Method.Body.Instructions)
			{
				switch (instruction.OpCode.Code)
				{
					case Code.Nop:
					case Code.Ret:
					case Code.Br_S:
						break;
					case Code.Brfalse:
					case Code.Brfalse_S:
						ctx.Stack.Pop();
						break;
					case Code.Call:
						Call(ctx, (MethodReference)instruction.Operand);
						break;
					case Code.Callvirt:
						Callvirt(ctx, (MethodReference)instruction.Operand);
						break;
					case Code.Ldarg:
						Ldarg(ctx, ((ParameterReference)instruction.Operand).Index);
						break;
					case Code.Ldarga_S:
						Ldarg(ctx, ((ParameterReference)instruction.Operand).Index);
						break;
					case Code.Ldarg_0:
						Ldarg(ctx, 0);
						break;
					case Code.Ldarg_1:
						Ldarg(ctx, 1);
						break;
					case Code.Ldarg_2:
						Ldarg(ctx, 2);
						break;
					case Code.Ldarg_3:
						Ldarg(ctx, 3);
						break;
					case Code.Ldfld:
						Ldfld(ctx, (FieldReference)instruction.Operand);
						break;
					case Code.Ldloc_0:
						Ldloc(ctx, 0);
						break;
					case Code.Ldloc_1:
						Ldloc(ctx, 1);
						break;
					case Code.Ldloc_2:
						Ldloc(ctx, 2);
						break;
					case Code.Ldloc_3:
						Ldloc(ctx, 3);
						break;
					case Code.Newobj:
						Newobj(ctx, (MethodReference)instruction.Operand);
						break;
					case Code.Stfld:
						Stfld(ctx, (FieldReference)instruction.Operand);
						break;
					case Code.Stloc_0:
						Stloc(ctx, 0);
						break;
					case Code.Stloc_1:
						Stloc(ctx, 1);
						break;
					case Code.Stloc_2:
						Stloc(ctx, 2);
						break;
					case Code.Stloc_3:
						Stloc(ctx, 3);
						break;
					default: throw new NotImplementedException($"Unknown instruction {instruction.OpCode}");
				}
			}
		}

		private static Ast[] PopArgs(RoutineContext ctx, MethodReference method)
		{
			Ast[] args = [];
			if (method.Parameters.Count > 0)
			{
				args = new Ast[method.Parameters.Count];
				for (var i = args.Length - 1; i >= 0; --i)
				{
					args[i] = ctx.Stack.Pop();
				}
			}
			return args;
		}

		private static void Call(RoutineContext ctx, MethodReference method)
		{
			var args = PopArgs(ctx, method);
			var call = new Cil.Call(method, args);

			ctx.Expressions.RemoveAll(args);
			ctx.Expressions.Add(call);

			if (method.ReturnType.MetadataType != MetadataType.Void)
				ctx.Stack.Push(call);
		}

		private static void Callvirt(RoutineContext ctx, MethodReference method)
		{
			var args = PopArgs(ctx, method);
			var obj = ctx.Stack.Pop();
			var callvirt = new Cil.Callvirt(obj, method, args);

			ctx.Expressions.RemoveAll(args);
			ctx.Expressions.Remove(obj);
			ctx.Expressions.Add(callvirt);

			if (method.ReturnType.MetadataType != MetadataType.Void)
				ctx.Stack.Push(callvirt);
		}

		private static void Newobj(RoutineContext ctx, MethodReference method)
		{
			var args = PopArgs(ctx, method);
			var newobj = new Cil.Newobj(method.DeclaringType, method, args);

			ctx.Expressions.RemoveAll(args);
			ctx.Expressions.Add(newobj);

			ctx.Stack.Push(newobj);
		}

		private static void Ldloc(RoutineContext ctx, int number)
		{
			ctx.Stack.Push(ctx.Variables[number]);
		}

		private static void Stloc(RoutineContext ctx, int number)
		{
			ctx.Variables[number] = ctx.Stack.Pop();
		}

		private static void Ldfld(RoutineContext ctx, FieldReference fld)
		{
			var obj = ctx.Stack.Pop();

			if (ctx.Memory.TryGetValue(Tuple.Create(obj, fld), out var value) == false)
				value = new Cil.Fld(obj, fld);

			ctx.Stack.Push(value);
		}

		private static void Stfld(RoutineContext ctx, FieldReference fld)
		{
			var value = ctx.Stack.Pop();
			var obj = ctx.Stack.Pop();
			ctx.Memory.AddOrReplace(Tuple.Create(obj, fld), value);
		}

		private static void Ldarg(RoutineContext ctx, int number)
		{
			if (ctx.Method.HasThis)
			{
				if (number == 0)
				{
					ctx.Stack.Push(new Cil.This(ctx.Method.DeclaringType));
				}
				else
				{
					var parameter = ctx.Method.Parameters[number - 1];
					ctx.Stack.Push(new Cil.Arg(number, parameter));
				}
			}
			else
			{
				var parameter = ctx.Method.Parameters[number];
				ctx.Stack.Push(new Cil.Arg(number, parameter));
			}
		}
		#endregion
	}
}
