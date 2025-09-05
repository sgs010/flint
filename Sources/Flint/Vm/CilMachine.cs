using Flint.Common;
using Flint.Vm.Cil;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm
{
	static class CilMachine
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
			foreach (var instruction in ctx.Method.Body.Instructions)
			{
				Eval(ctx, instruction);
			}
			return ctx.Expressions.ToList();
		}
		#endregion

		#region Implementation
		internal readonly struct ArrayIndex
		{
			public readonly Ast Array;
			public readonly Ast Index;
			public ArrayIndex(Ast array, Ast index)
			{
				Array = array;
				Index = index;
			}

			public override readonly bool Equals(object obj)
			{
				if (obj is ArrayIndex ai)
				{
					return Array.Equals(ai.Array)
						&& Index.Equals(ai.Index);
				}
				return false;
			}

			public override readonly int GetHashCode()
			{
				return HashCode.Combine(Array, Index);
			}
		}

		internal readonly struct ObjectField
		{
			public readonly Ast Object;
			public readonly FieldReference Field;
			public ObjectField(Ast instance, FieldReference fld)
			{
				Object = instance;
				Field = fld;
			}

			public override readonly bool Equals(object obj)
			{
				if (obj is ObjectField of)
				{
					return (Object != null ? Object.Equals(of.Object) : of.Object is null)
						&& Field.Equals(of.Field);
				}
				return false;
			}

			public override readonly int GetHashCode()
			{
				return HashCode.Combine(Object, Field);
			}
		}

		internal class RoutineContext
		{
			public readonly MethodDefinition Method;
			public readonly Ast[] Args;
			public readonly Ast[] Vars;
			public readonly Stack<Ast> Stack;
			public readonly Dictionary<Ast, Ast> Heap = [];
			public readonly Dictionary<ArrayIndex, Ast> Arrays = [];
			public readonly Dictionary<ObjectField, Ast> Objects = [];
			public readonly HashSet<Ast> Expressions = [];
			public readonly Instruction[] ExceptionHandlers;

			public RoutineContext(MethodDefinition method)
			{
				Method = method;
				Args = new Ast[method.Parameters.Count];
				Vars = new Ast[method.Body.Variables.Count];
				Stack = new Stack<Ast>(method.Body.MaxStackSize);
				ExceptionHandlers = method.Body.ExceptionHandlers
					.Where(x => x.HandlerType == ExceptionHandlerType.Catch)
					.Select(x => x.HandlerStart)
					.ToArray();
			}

			// use for tests only
			internal RoutineContext(MethodDefinition method = null, int varCount = 0, int stackSize = 0, Instruction[] exceptionHandlers = null)
			{
				Method = method;
				Args = new Ast[method?.Parameters.Count ?? 0];
				Vars = new Ast[varCount];
				Stack = new Stack<Ast>(stackSize);
				ExceptionHandlers = exceptionHandlers ?? [];
			}
		}

		internal static void Eval(RoutineContext ctx, Instruction instruction)
		{
			if (ctx.ExceptionHandlers.Any(x => x.Offset == instruction.Offset))
			{
				// vm puts exceptions on stack
				// we must do the same if we enter exception catch block
				ctx.Stack.Push(Cil.Exception.Instance);
			}

			switch (instruction.OpCode.Code)
			{
				case Code.Add:
				case Code.Add_Ovf:
				case Code.Add_Ovf_Un:
					Add(ctx);
					break;
				case Code.And:
					And(ctx);
					break;
				case Code.Arglist:
					Arglist(ctx);
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
				case Code.Bne_Un:
				case Code.Bne_Un_S:
					ctx.Stack.Pop();
					ctx.Stack.Pop();
					break;
				case Code.Box:
					Box(ctx);
					break;
				case Code.Br:
				case Code.Br_S:
					break;
				case Code.Break:
					break;
				case Code.Brfalse:
				case Code.Brfalse_S:
				case Code.Brtrue:
				case Code.Brtrue_S:
					ctx.Stack.Pop();
					break;
				case Code.Call:
				case Code.Callvirt:
					Call(ctx, (MethodReference)instruction.Operand);
					break;
				case Code.Castclass:
					Castclass(ctx, (TypeReference)instruction.Operand);
					break;
				case Code.Ceq:
					Ceq(ctx);
					break;
				case Code.Cgt:
				case Code.Cgt_Un:
					Cgt(ctx);
					break;
				case Code.Ckfinite:
					break;
				case Code.Clt:
				case Code.Clt_Un:
					Clt(ctx);
					break;
				case Code.Constrained:
					break;
				case Code.Conv_I:
				case Code.Conv_Ovf_I:
				case Code.Conv_Ovf_I_Un:
					Conv_I(ctx);
					break;
				case Code.Conv_I1:
				case Code.Conv_Ovf_I1:
				case Code.Conv_Ovf_I1_Un:
					Conv_I1(ctx);
					break;
				case Code.Conv_I2:
				case Code.Conv_Ovf_I2:
				case Code.Conv_Ovf_I2_Un:
					Conv_I2(ctx);
					break;
				case Code.Conv_I4:
				case Code.Conv_Ovf_I4:
				case Code.Conv_Ovf_I4_Un:
					Conv_I4(ctx);
					break;
				case Code.Conv_I8:
				case Code.Conv_Ovf_I8:
				case Code.Conv_Ovf_I8_Un:
					Conv_I8(ctx);
					break;
				case Code.Conv_R_Un:
				case Code.Conv_R4:
					Conv_R4(ctx);
					break;
				case Code.Conv_R8:
					Conv_R8(ctx);
					break;
				case Code.Conv_U:
					Conv_U(ctx);
					break;
				case Code.Conv_U1:
					Conv_U1(ctx);
					break;
				case Code.Conv_U2:
					Conv_U2(ctx);
					break;
				case Code.Conv_U4:
					Conv_U4(ctx);
					break;
				case Code.Conv_U8:
					Conv_U8(ctx);
					break;
				case Code.Cpblk:
					Cpblk(ctx);
					break;
				case Code.Cpobj:
					Cpobj(ctx);
					break;
				case Code.Div:
				case Code.Div_Un:
					Div(ctx);
					break;
				case Code.Dup:
					ctx.Stack.Push(ctx.Stack.Peek());
					break;
				case Code.Endfilter:
				case Code.Endfinally:
					break;
				case Code.Initblk:
					Initblk(ctx);
					break;
				case Code.Initobj:
					Initobj(ctx, (TypeReference)instruction.Operand);
					break;
				case Code.Isinst:
					Isinst(ctx, (TypeReference)instruction.Operand);
					break;
				case Code.Jmp:
					break;
				case Code.Ldarg:
				case Code.Ldarg_S:
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
				case Code.Ldarga:
				case Code.Ldarga_S:
					Ldarga(ctx, ((ParameterReference)instruction.Operand).Index);
					break;
				case Code.Ldc_I4:
					Ldc_I4(ctx, (int)instruction.Operand);
					break;
				case Code.Ldc_I4_0:
					Ldc_I4(ctx, 0);
					break;
				case Code.Ldc_I4_1:
					Ldc_I4(ctx, 1);
					break;
				case Code.Ldc_I4_2:
					Ldc_I4(ctx, 2);
					break;
				case Code.Ldc_I4_3:
					Ldc_I4(ctx, 3);
					break;
				case Code.Ldc_I4_4:
					Ldc_I4(ctx, 4);
					break;
				case Code.Ldc_I4_5:
					Ldc_I4(ctx, 5);
					break;
				case Code.Ldc_I4_6:
					Ldc_I4(ctx, 6);
					break;
				case Code.Ldc_I4_7:
					Ldc_I4(ctx, 7);
					break;
				case Code.Ldc_I4_8:
					Ldc_I4(ctx, 8);
					break;
				case Code.Ldc_I4_M1:
					Ldc_I4(ctx, -1);
					break;
				case Code.Ldc_I4_S:
					Ldc_I4(ctx, (sbyte)instruction.Operand);
					break;
				case Code.Ldc_I8:
					Ldc_I8(ctx, (long)instruction.Operand);
					break;
				case Code.Ldc_R4:
					Ldc_R4(ctx, (float)instruction.Operand);
					break;
				case Code.Ldc_R8:
					Ldc_R8(ctx, (double)instruction.Operand);
					break;
				case Code.Ldelem_Any:
				case Code.Ldelem_I:
				case Code.Ldelem_I1:
				case Code.Ldelem_I2:
				case Code.Ldelem_I4:
				case Code.Ldelem_I8:
				case Code.Ldelem_R4:
				case Code.Ldelem_R8:
				case Code.Ldelem_Ref:
				case Code.Ldelem_U1:
				case Code.Ldelem_U2:
				case Code.Ldelem_U4:
					Ldelem(ctx);
					break;
				case Code.Ldelema:
					Ldelema(ctx);
					break;
				case Code.Ldfld:
				case Code.Ldflda:
				case Code.Ldsfld:
				case Code.Ldsflda:
					Ldfld(ctx, (FieldDefinition)instruction.Operand);
					break;
				case Code.Ldftn:
					Ldftn(ctx, (MethodDefinition)instruction.Operand);
					break;
				case Code.Ldind_I:
				case Code.Ldind_I1:
				case Code.Ldind_I2:
				case Code.Ldind_I4:
				case Code.Ldind_I8:
				case Code.Ldind_R4:
				case Code.Ldind_R8:
				case Code.Ldind_Ref:
				case Code.Ldind_U1:
				case Code.Ldind_U2:
				case Code.Ldind_U4:
					Ldind(ctx);
					break;
				case Code.Ldlen:
					Ldlen(ctx);
					break;
				case Code.Ldloc:
				case Code.Ldloc_S:
					Ldloc(ctx, ((VariableReference)instruction.Operand).Index);
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
				case Code.Ldloca:
				case Code.Ldloca_S:
					Ldloca(ctx, (VariableReference)instruction.Operand);
					break;
				case Code.Ldnull:
					ctx.Stack.Push(Cil.Null.Instance);
					break;
				case Code.Ldobj:
					Ldind(ctx);
					break;
				case Code.Ldstr:
					Ldstr(ctx, (string)instruction.Operand);
					break;
				case Code.Ldtoken:
					Ldtoken(ctx, instruction.Operand);
					break;
				case Code.Ldvirtftn:
					Ldvirtftn(ctx, (MethodDefinition)instruction.Operand);
					break;
				case Code.Leave:
				case Code.Leave_S:
					break;
				case Code.Localloc:
					Localloc(ctx);
					break;
				case Code.Mkrefany:
					Mkref(ctx, (TypeReference)instruction.Operand);
					break;
				case Code.Mul:
				case Code.Mul_Ovf:
				case Code.Mul_Ovf_Un:
					Mul(ctx);
					break;
				case Code.Neg:
					Neg(ctx);
					break;
				case Code.Newarr:
					Newarr(ctx, (TypeReference)instruction.Operand);
					break;
				case Code.Newobj:
					Newobj(ctx, (MethodReference)instruction.Operand);
					break;
				case Code.Nop:
					break;
				case Code.Not:
					Not(ctx);
					break;
				case Code.Or:
					Or(ctx);
					break;
				case Code.Pop:
					ctx.Stack.Pop();
					break;
				case Code.Readonly:
					break;
				case Code.Refanytype:
					Refanytype(ctx);
					break;
				case Code.Refanyval:
					Refanyval(ctx, (TypeReference)instruction.Operand);
					break;
				case Code.Rem:
				case Code.Rem_Un:
					Rem(ctx);
					break;
				case Code.Ret:
				case Code.Rethrow:
					break;
				case Code.Shl:
					Shl(ctx);
					break;
				case Code.Shr:
				case Code.Shr_Un:
					Shr(ctx);
					break;
				case Code.Sizeof:
					Sizeof(ctx, (TypeReference)instruction.Operand);
					break;
				case Code.Starg:
				case Code.Starg_S:
					Starg(ctx, ((ParameterReference)instruction.Operand).Index);
					break;
				case Code.Stelem_Any:
				case Code.Stelem_I:
				case Code.Stelem_I1:
				case Code.Stelem_I2:
				case Code.Stelem_I4:
				case Code.Stelem_I8:
				case Code.Stelem_R4:
				case Code.Stelem_R8:
				case Code.Stelem_Ref:
					Stelem(ctx);
					break;
				case Code.Stfld:
				case Code.Stsfld:
					Stfld(ctx, (FieldDefinition)instruction.Operand);
					break;
				case Code.Stind_I:
				case Code.Stind_I1:
				case Code.Stind_I2:
				case Code.Stind_I4:
				case Code.Stind_I8:
				case Code.Stind_R4:
				case Code.Stind_R8:
				case Code.Stind_Ref:
					Stind(ctx);
					break;
				case Code.Stloc:
				case Code.Stloc_S:
					Stloc(ctx, ((VariableReference)instruction.Operand).Index);
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
				case Code.Stobj:
					Stobj(ctx);
					break;
				case Code.Sub:
				case Code.Sub_Ovf:
				case Code.Sub_Ovf_Un:
					Sub(ctx);
					break;
				case Code.Switch:
					ctx.Stack.Pop();
					break;
				case Code.Tail:
					break;
				case Code.Throw:
					ctx.Stack.Pop();
					break;
				case Code.Unaligned:
					Unaligned(ctx, (byte)instruction.Operand);
					break;
				case Code.Unbox:
				case Code.Unbox_Any:
					Unbox(ctx, (TypeReference)instruction.Operand);
					break;
				case Code.Volatile:
					break;
				case Code.Xor:
					Xor(ctx);
					break;
				default: throw new NotImplementedException($"Unknown instruction {instruction.OpCode.Code}");
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

		private static void Add(RoutineContext ctx)
		{
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Add(left, right));
		}

		private static void And(RoutineContext ctx)
		{
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.And(left, right));
		}

		private static void Arglist(RoutineContext ctx)
		{
			ctx.Stack.Push(new Cil.Arglist(ctx.Method));
		}

		private static void Box(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Box(value));
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

			for (var i = 0; i < args.Length; ++i)
			{
				var arg = args[i];
				if (arg is Varptr var)
					ctx.Vars[var.Index] = new Cil.OutArg(call, i);
			}
		}

		private static void Castclass(RoutineContext ctx, TypeReference type)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Castclass(type, value));
		}

		private static void Ceq(RoutineContext ctx)
		{
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Ceq(left, right));
		}

		private static void Cgt(RoutineContext ctx)
		{
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Cgt(left, right));
		}

		private static void Clt(RoutineContext ctx)
		{
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Clt(left, right));
		}

		private static void Conv_I(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_I(value));
		}

		private static void Conv_I1(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_I1(value));
		}

		private static void Conv_I2(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_I2(value));
		}

		private static void Conv_I4(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_I4(value));
		}

		private static void Conv_I8(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_I8(value));
		}

		private static void Conv_R4(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_R4(value));
		}

		private static void Conv_R8(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_R8(value));
		}

		private static void Conv_U(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_U(value));
		}

		private static void Conv_U1(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_U1(value));
		}

		private static void Conv_U2(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_U2(value));
		}

		private static void Conv_U4(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_U4(value));
		}

		private static void Conv_U8(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_U8(value));
		}

		private static void Cpblk(RoutineContext ctx)
		{
			var count = ctx.Stack.Pop();
			var src = ctx.Stack.Pop();
			var dest = ctx.Stack.Pop();
		}

		private static void Cpobj(RoutineContext ctx)
		{
			var src = ctx.Stack.Pop();
			var dest = ctx.Stack.Pop();
		}

		private static void Div(RoutineContext ctx)
		{
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Div(left, right));
		}

		private static void Initblk(RoutineContext ctx)
		{
			var count = ctx.Stack.Pop();
			var value = ctx.Stack.Pop();
			var address = ctx.Stack.Pop();
		}

		private static void Initobj(RoutineContext ctx, TypeReference type)
		{
			var address = ctx.Stack.Pop();
		}

		public static void Isinst(RoutineContext ctx, TypeReference type)
		{
			var instance = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Isinst(type, instance));
		}

		private static void Ldarg(RoutineContext ctx, int number)
		{
			if (ctx.Method.HasThis && number == 0)
			{
				ctx.Stack.Push(new Cil.This(ctx.Method.DeclaringType));
				return;
			}

			var argNum = ctx.Method.HasThis ? number - 1 : number;
			var value = ctx.Args[argNum];
			if (value == null)
			{
				var parameter = ctx.Method.Parameters[argNum];
				value = new Cil.Arg(argNum, parameter);
			}
			ctx.Stack.Push(value);
		}

		private static void Ldarga(RoutineContext ctx, int number)
		{
			ctx.Stack.Push(new Cil.Argptr(number));
		}

		private static void Ldc_I4(RoutineContext ctx, int value)
		{
			ctx.Stack.Push(new Cil.Int32(value));
		}

		private static void Ldc_I8(RoutineContext ctx, long value)
		{
			ctx.Stack.Push(new Cil.Int64(value));
		}

		private static void Ldc_R4(RoutineContext ctx, float value)
		{
			ctx.Stack.Push(new Cil.Float32(value));
		}

		private static void Ldc_R8(RoutineContext ctx, double value)
		{
			ctx.Stack.Push(new Cil.Float64(value));
		}

		private static void Ldelem(RoutineContext ctx)
		{
			var index = ctx.Stack.Pop();
			var array = ctx.Stack.Pop();

			if (ctx.Arrays.TryGetValue(new ArrayIndex(array, index), out var value) == false)
				value = new Cil.Elem(array, index);

			ctx.Stack.Push(value);
		}

		private static void Ldelema(RoutineContext ctx)
		{
			var index = ctx.Stack.Pop();
			var array = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Elemptr(array, index));
		}

		private static void Ldfld(RoutineContext ctx, FieldDefinition fld)
		{
			Ast instance = null;
			if (fld.IsStatic == false)
				instance = ctx.Stack.Pop();

			if (ctx.Objects.TryGetValue(new ObjectField(instance, fld), out var value) == false)
				value = new Cil.Fld(instance, fld);

			ctx.Stack.Push(value);
		}

		private static void Ldftn(RoutineContext ctx, MethodDefinition mtd)
		{
			ctx.Stack.Push(new Cil.Ftn(null, mtd));
		}

		private static void Ldind(RoutineContext ctx)
		{
			var address = ctx.Stack.Pop();
			if (ctx.Heap.TryGetValue(address, out var value) == false)
				value = new Cil.Heapptr(address);
			ctx.Stack.Push(value);
		}

		private static void Ldlen(RoutineContext ctx)
		{
			var array = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Len(array));
		}

		private static void Ldloc(RoutineContext ctx, int number)
		{
			ctx.Stack.Push(ctx.Vars[number]);
		}

		private static void Ldloca(RoutineContext ctx, VariableReference v)
		{
			var value = ctx.Vars[v.Index];
			if (value != null)
				ctx.Stack.Push(value);
			else
				ctx.Stack.Push(new Cil.Varptr(v.Index));
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

		private static void Ldvirtftn(RoutineContext ctx, MethodDefinition mtd)
		{
			var instance = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Ftn(instance, mtd));
		}

		private static void Localloc(RoutineContext ctx)
		{
			var count = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Bytes(count));
		}

		private static void Mkref(RoutineContext ctx, TypeReference type)
		{
			var address = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Mkref(address, type));
		}

		private static void Mul(RoutineContext ctx)
		{
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Mul(left, right));
		}

		private static void Neg(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Neg(value));
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

		private static void Not(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Not(value));
		}

		private static void Or(RoutineContext ctx)
		{
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Or(left, right));
		}

		private static void Refanytype(RoutineContext ctx)
		{
			var reference = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Refanytype(reference));
		}

		private static void Refanyval(RoutineContext ctx, TypeReference type)
		{
			var address = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Refanyval(type, address));
		}

		private static void Rem(RoutineContext ctx)
		{
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Rem(left, right));
		}

		private static void Shl(RoutineContext ctx)
		{
			var count = ctx.Stack.Pop();
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Shl(value, count));
		}

		private static void Shr(RoutineContext ctx)
		{
			var count = ctx.Stack.Pop();
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Shr(value, count));
		}

		private static void Sizeof(RoutineContext ctx, TypeReference type)
		{
			ctx.Stack.Push(new Cil.Sizeof(type));
		}

		private static void Starg(RoutineContext ctx, int number)
		{
			if (ctx.Method.HasThis && number == 0)
				throw new InvalidOperationException();

			var argNum = ctx.Method.HasThis ? number - 1 : number;
			var value = ctx.Stack.Pop();
			ctx.Args[argNum] = value;
		}

		private static void Stelem(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			var index = ctx.Stack.Pop();
			var array = ctx.Stack.Pop();
			ctx.Arrays.AddOrReplace(new ArrayIndex(array, index), value);
		}

		private static void Stind(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			var address = ctx.Stack.Pop();
			ctx.Heap.AddOrReplace(address, value);
		}

		private static void Stloc(RoutineContext ctx, int number)
		{
			ctx.Vars[number] = ctx.Stack.Pop();
		}

		private static void Stobj(RoutineContext ctx)
		{
			var value = ctx.Stack.Pop();
			var address = ctx.Stack.Pop();
			ctx.Heap.AddOrReplace(address, value);
		}

		private static void Stfld(RoutineContext ctx, FieldDefinition fld)
		{
			var value = ctx.Stack.Pop();

			Ast instance = null;
			if (fld.IsStatic == false)
				instance = ctx.Stack.Pop();

			ctx.Objects.AddOrReplace(new ObjectField(instance, fld), value);
		}

		private static void Sub(RoutineContext ctx)
		{
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Sub(left, right));
		}

		private static void Unaligned(RoutineContext ctx, byte address)
		{
			ctx.Stack.Push(new Cil.Unaligned(address));
		}

		private static void Unbox(RoutineContext ctx, TypeReference type)
		{
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Unbox(type, value));
		}

		private static void Xor(RoutineContext ctx)
		{
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Xor(left, right));
		}
		#endregion
	}
}
