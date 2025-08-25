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
		readonly struct MemKey
		{
			public readonly Ast Instance;
			public readonly FieldReference Field;
			public MemKey(Ast instance, FieldReference fld)
			{
				Instance = instance;
				Field = fld;
			}

			public override readonly bool Equals(object obj)
			{
				if (obj is MemKey mk)
				{
					return Instance.Equals(mk.Instance)
						&& Field.Equals(mk.Field);
				}
				return false;
			}

			public override readonly int GetHashCode()
			{
				return HashCode.Combine(Instance, Field);
			}
		}

		class RoutineContext
		{
			public readonly MethodDefinition Method;
			public readonly Dictionary<MemKey, Ast> Memory;
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
				if (ctx.Method.Body.HasExceptionHandlers)
				{
					// vm puts exceptions on stack
					// we must do the same if we enter exception catch block
					if (ctx.Method.Body.ExceptionHandlers.Any(x => x.HandlerType == ExceptionHandlerType.Catch
																&& x.HandlerStart.Offset == instruction.Offset))
						ctx.Stack.Push(Cil.Exception.Instance);
				}

				switch (instruction.OpCode.Code)
				{
					case Code.Nop:
					case Code.Constrained:
					case Code.Ret:
					case Code.Leave:
					case Code.Leave_S:
					case Code.Br:
					case Code.Br_S:
					case Code.Endfinally:
						break;
					case Code.Pop:
					case Code.Switch:
					case Code.Brfalse:
					case Code.Brfalse_S:
					case Code.Brtrue:
					case Code.Brtrue_S:
						ctx.Stack.Pop();
						break;
					case Code.Beq:
					case Code.Beq_S:
					case Code.Bge:
					case Code.Bge_S:
					case Code.Bge_Un:
					case Code.Bge_Un_S:
					case Code.Bgt:
					case Code.Bgt_S:
					case Code.Bgt_Un:
					case Code.Bgt_Un_S:
					case Code.Ble:
					case Code.Ble_S:
					case Code.Ble_Un:
					case Code.Ble_Un_S:
					case Code.Blt:
					case Code.Blt_S:
					case Code.Blt_Un:
					case Code.Blt_Un_S:
						ctx.Stack.Pop();
						ctx.Stack.Pop();
						break;
					case Code.Box:
						Box(ctx);
						break;
					case Code.Dup:
						ctx.Stack.Push(ctx.Stack.Peek());
						break;
					case Code.Call:
					case Code.Callvirt:
						Call(ctx, (MethodReference)instruction.Operand);
						break;
					case Code.Castclass:
						CastClass(ctx, (TypeReference)instruction.Operand);
						break;
					case Code.Initobj:
						ctx.Stack.Pop();
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
					case Code.Ldc_I4:
						LdcI4(ctx, (int)instruction.Operand);
						break;
					case Code.Ldc_I4_0:
						LdcI4(ctx, 0);
						break;
					case Code.Ldc_I4_1:
						LdcI4(ctx, 1);
						break;
					case Code.Ldc_I4_2:
						LdcI4(ctx, 2);
						break;
					case Code.Ldc_I4_3:
						LdcI4(ctx, 3);
						break;
					case Code.Ldc_I4_4:
						LdcI4(ctx, 4);
						break;
					case Code.Ldc_I4_5:
						LdcI4(ctx, 5);
						break;
					case Code.Ldc_I4_6:
						LdcI4(ctx, 6);
						break;
					case Code.Ldc_I4_7:
						LdcI4(ctx, 7);
						break;
					case Code.Ldc_I4_8:
						LdcI4(ctx, 8);
						break;
					case Code.Ldc_I4_M1:
						LdcI4(ctx, -1);
						break;
					case Code.Ldc_I4_S:
						LdcI4(ctx, (SByte)instruction.Operand);
						break;
					case Code.Ldfld:
					case Code.Ldflda:
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
					case Code.Ldloc_S:
						Ldloc(ctx, ((VariableReference)instruction.Operand).Index);
						break;
					case Code.Ldloca_S:
						Ldloca(ctx, (VariableReference)instruction.Operand);
						break;
					case Code.Ldnull:
						ctx.Stack.Push(Cil.Null.Instance);
						break;
					case Code.Ldstr:
						Ldstr(ctx, (string)instruction.Operand);
						break;
					case Code.Ldtoken:
						Ldtoken(ctx, instruction.Operand);
						break;
					case Code.Newarr:
						Newarr(ctx, (TypeReference)instruction.Operand);
						break;
					case Code.Newobj:
						Newobj(ctx, (MethodReference)instruction.Operand);
						break;
					case Code.Stfld:
						Stfld(ctx, (FieldReference)instruction.Operand);
						break;
					case Code.Stelem_Ref:
						Stelem(ctx);
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
					case Code.Stloc_S:
						Stloc(ctx, ((VariableReference)instruction.Operand).Index);
						break;
					default: throw new NotImplementedException($"Unknown instruction {instruction.OpCode.Code}");
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

			Ast instance = null;
			if (method.HasThis)
				instance = ctx.Stack.Pop();

			var call = new Cil.Call(instance, method, args);

			ctx.Expressions.RemoveAll(args);
			if (instance != null)
				ctx.Expressions.Remove(instance);
			ctx.Expressions.Add(call);

			if (method.ReturnType.MetadataType != MetadataType.Void)
				ctx.Stack.Push(call);
		}

		private static void Newarr(RoutineContext ctx, TypeReference type)
		{
			var size = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Array(type, size));
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

		private static void Ldloca(RoutineContext ctx, VariableReference v)
		{
			var value = ctx.Variables[v.Index];
			if (value != null)
				ctx.Stack.Push(value);
			else
				ctx.Stack.Push(new Cil.Var(v.Index, v.VariableType));
		}

		private static void Ldfld(RoutineContext ctx, FieldReference fld)
		{
			var obj = ctx.Stack.Pop();

			if (ctx.Memory.TryGetValue(new MemKey(obj, fld), out var value) == false)
				value = new Cil.Fld(obj, fld);

			ctx.Stack.Push(value);
		}

		private static void Stfld(RoutineContext ctx, FieldReference fld)
		{
			var value = ctx.Stack.Pop();
			var obj = ctx.Stack.Pop();
			ctx.Memory.AddOrReplace(new MemKey(obj, fld), value);
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

		private static void LdcI4(RoutineContext ctx, int value)
		{
			ctx.Stack.Push(new Cil.Int32(value));
		}

		private static void Ldstr(RoutineContext ctx, string value)
		{
			ctx.Stack.Push(new Cil.String(value));
		}

		private static void Ldtoken(RoutineContext ctx, object value)
		{
			Ast token;
			if (value is TypeReference t)
				token = new Cil.Typeof(t);
			else if (value is MethodReference m)
				token = new Cil.Methodof(m);
			else throw new NotImplementedException($"Unknown token {value}");

			ctx.Stack.Push(token);
		}

		private static void CastClass(RoutineContext ctx, TypeReference type)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Cast(type, value));
		}

		private static void Stelem(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			var index = ctx.Stack.Pop();
			var array = ctx.Stack.Pop();
			((Cil.Array)array).Elements.AddOrReplace(index, value);
		}

		private static void Box(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Box(value));
		}
		#endregion
	}
}
