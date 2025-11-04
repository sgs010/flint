using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using Flint.Common;
using Flint.Vm.Cil;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm
{
	#region ICilMachineContext
	interface ICilMachineContext
	{
		string GetMethodFullName(MethodReference method);
	}
	#endregion

	#region CilMachine
	static class CilMachine
	{
		#region Interface
		public static ImmutableArray<Ast> Run(MethodDefinition mtd, ICilMachineContext machineContext = null)
		{
			// general idea: avoid combinatory explosion when a method has a lot of branches
			// 1. eval branch and get a list of alt branches from it
			// 2. merge branch expressions with prev branch expressions
			// 3. if nothing changed, then ignore alt branches

			var expressions = new Dictionary<CilPoint, Ast>();
			var visitedBranches = new HashSet<int>();
			var queue = new Queue<RoutineContext>();
			queue.Enqueue(new RoutineContext(mtd));
			while (queue.Count > 0)
			{
				var branch = queue.Dequeue();
				visitedBranches.Add(branch.StartInstruction.Offset);

				var altBranches = new List<RoutineContext>();
				Eval(branch, altBranches, machineContext);

				// check if this branch gives us any new expressions
				var isChanged = Merge(expressions, branch.Expressions);
				if (isChanged)
				{
					// got new expressions, process all alt branches
					foreach (var b in altBranches)
						queue.Enqueue(b);
					continue;
				}

				// got no new expressions, but possibly could get new unvisited branches - process them too
				foreach (var b in altBranches)
				{
					if (visitedBranches.Contains(b.StartInstruction.Offset) == false)
						queue.Enqueue(b);
				}
			}

			return [.. expressions.Values];
		}
		#endregion

		#region Implementation
		[DebuggerDisplay("IL_{Ast.CilPoint.Offset.ToString(\"X4\").ToLower()}: {Value}")]
		internal readonly struct Condition
		{
			public readonly Ast Ast;
			public readonly int Value;
			public Condition(Ast ast, int value)
			{
				Ast = ast;
				Value = value;
			}
		}

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

		[DebuggerDisplay("{StartInstruction}")]
		internal class RoutineContext
		{
			public readonly MethodDefinition Method;
			public readonly Instruction StartInstruction;
			public readonly ImmutableArray<Instruction> ExceptionHandlers;
			public readonly ImmutableArray<SequencePoint> SequencePoints;

			public readonly Ast[] Args;
			public readonly Ast[] Vars;
			public readonly Stack<Ast> Stack;
			public readonly Dictionary<Ast, Ast> Heap = [];
			public readonly Dictionary<ArrayIndex, Ast> Arrays = [];
			public readonly Dictionary<ObjectField, Ast> Objects = [];
			public readonly List<Condition> Conditions = [];
			public readonly HashSet<Ast> Expressions = [];
			public readonly HashSet<int> VisitedOffsets = [];

			public RoutineContext(MethodDefinition method)
			{
				Method = method;
				StartInstruction = Method.Body.Instructions.First();
				Args = new Ast[method.Parameters.Count];
				Vars = new Ast[method.Body.Variables.Count];
				Stack = new Stack<Ast>(method.Body.MaxStackSize);

				ExceptionHandlers = method.Body.ExceptionHandlers
					.Where(x => x.HandlerType == ExceptionHandlerType.Catch)
					.Select(x => x.HandlerStart)
					.ToImmutableArray();

				try
				{
					SequencePoints = method.DebugInformation.SequencePoints
						.Where(x => x.IsHidden == false)
						.ToImmutableArray();
				}
				catch (System.Exception) { }
			}

			[Obsolete("for tests only")]
			internal RoutineContext(MethodDefinition method = null, int varCount = 0, int stackSize = 0, Instruction[] exceptionHandlers = null)
			{
				Method = method;
				Args = new Ast[method?.Parameters.Count ?? 0];
				Vars = new Ast[varCount];
				Stack = new Stack<Ast>(stackSize);
				ExceptionHandlers = [.. exceptionHandlers ?? []];
			}

			internal RoutineContext(RoutineContext src, Instruction start)
			{
				Method = src.Method;
				StartInstruction = start;
				ExceptionHandlers = src.ExceptionHandlers;
				SequencePoints = src.SequencePoints;

				Args = [.. src.Args];
				Vars = [.. src.Vars];
				Stack = new Stack<Ast>(src.Stack.ToArray().Reverse());
				Heap = new Dictionary<Ast, Ast>(src.Heap);
				Arrays = new Dictionary<ArrayIndex, Ast>(src.Arrays);
				Objects = new Dictionary<ObjectField, Ast>(src.Objects);
				Conditions = [.. src.Conditions];
				Expressions = [.. src.Expressions];
				VisitedOffsets = [.. src.VisitedOffsets];
			}
		}

		private static bool Merge(Dictionary<CilPoint, Ast> acc, IEnumerable<Ast> expressions)
		{
			var isChanged = false;
			foreach (var expr in expressions)
			{
				var pt = expr.CilPoint;
				if (acc.TryGetValue(pt, out var prevExpr))
				{
					// merge
					var (merged, mergeResult) = Ast.Merge(expr, prevExpr);
					if (mergeResult != Ast.MergeResult.OkMerged)
						continue;
					if (Are.Equal(prevExpr, merged))
						continue;
					acc[pt] = merged;
					isChanged = true;
				}
				else
				{
					// add
					acc.Add(pt, expr);
					isChanged = true;
				}
			}
			return isChanged;
		}

		private static void Eval(RoutineContext branch, List<RoutineContext> altBranches, ICilMachineContext machineContext)
		{
			var instruction = branch.StartInstruction;
			while (instruction != null)
			{
				if (branch.VisitedOffsets.Contains(instruction.Offset))
					break; // avoid loops

				Eval(branch, altBranches, instruction, out var nextInstruction, machineContext);
				branch.VisitedOffsets.Add(instruction.Offset);
				instruction = nextInstruction;
			}
		}

		[Obsolete("for tests only")]
		internal static void Eval(RoutineContext ctx, Instruction instruction)
		{
			Eval(ctx, [], instruction, out var _, null);
		}

		[Obsolete("for tests only")]
		internal static void Eval(RoutineContext ctx, List<RoutineContext> branches, Instruction instruction, out Instruction nextInstruction)
		{
			Eval(ctx, branches, instruction, out nextInstruction, null);
		}

		private static void Eval(RoutineContext ctx, List<RoutineContext> branches, Instruction instruction, out Instruction nextInstruction, ICilMachineContext machineContext)
		{
			nextInstruction = instruction.Next;

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
					Add(ctx, instruction);
					break;
				case Code.And:
					And(ctx, instruction);
					break;
				case Code.Arglist:
					Arglist(ctx, instruction);
					break;
				case Code.Beq:
				case Code.Beq_S:
					Branch(Beq.Create, ctx, branches, instruction, out nextInstruction);
					break;
				case Code.Bge:
				case Code.Bge_S:
				case Code.Bge_Un:
				case Code.Bge_Un_S:
					Branch(Bge.Create, ctx, branches, instruction, out nextInstruction);
					break;
				case Code.Bgt:
				case Code.Bgt_S:
				case Code.Bgt_Un:
				case Code.Bgt_Un_S:
					Branch(Bgt.Create, ctx, branches, instruction, out nextInstruction);
					break;
				case Code.Ble:
				case Code.Ble_S:
				case Code.Ble_Un:
				case Code.Ble_Un_S:
					Branch(Ble.Create, ctx, branches, instruction, out nextInstruction);
					break;
				case Code.Blt:
				case Code.Blt_S:
				case Code.Blt_Un:
				case Code.Blt_Un_S:
					Branch(Blt.Create, ctx, branches, instruction, out nextInstruction);
					break;
				case Code.Bne_Un:
				case Code.Bne_Un_S:
					Branch(Bne.Create, ctx, branches, instruction, out nextInstruction);
					break;
				case Code.Box:
					Box(ctx, instruction);
					break;
				case Code.Br:
				case Code.Br_S:
					nextInstruction = (Instruction)instruction.Operand;
					break;
				case Code.Break:
					break;
				case Code.Brfalse:
				case Code.Brfalse_S:
					Branch(Brfalse.Create, ctx, branches, instruction, out nextInstruction);
					break;
				case Code.Brtrue:
				case Code.Brtrue_S:
					Branch(Brtrue.Create, ctx, branches, instruction, out nextInstruction);
					break;
				case Code.Call:
				case Code.Callvirt:
					Call(ctx, instruction, machineContext);
					break;
				case Code.Castclass:
					Castclass(ctx, instruction);
					break;
				case Code.Ceq:
					Ceq(ctx, instruction);
					break;
				case Code.Cgt:
				case Code.Cgt_Un:
					Cgt(ctx, instruction);
					break;
				case Code.Ckfinite:
					break;
				case Code.Clt:
				case Code.Clt_Un:
					Clt(ctx, instruction);
					break;
				case Code.Constrained:
					break;
				case Code.Conv_I:
				case Code.Conv_Ovf_I:
				case Code.Conv_Ovf_I_Un:
					Conv_I(ctx, instruction);
					break;
				case Code.Conv_I1:
				case Code.Conv_Ovf_I1:
				case Code.Conv_Ovf_I1_Un:
					Conv_I1(ctx, instruction);
					break;
				case Code.Conv_I2:
				case Code.Conv_Ovf_I2:
				case Code.Conv_Ovf_I2_Un:
					Conv_I2(ctx, instruction);
					break;
				case Code.Conv_I4:
				case Code.Conv_Ovf_I4:
				case Code.Conv_Ovf_I4_Un:
					Conv_I4(ctx, instruction);
					break;
				case Code.Conv_I8:
				case Code.Conv_Ovf_I8:
				case Code.Conv_Ovf_I8_Un:
					Conv_I8(ctx, instruction);
					break;
				case Code.Conv_R_Un:
				case Code.Conv_R4:
					Conv_R4(ctx, instruction);
					break;
				case Code.Conv_R8:
					Conv_R8(ctx, instruction);
					break;
				case Code.Conv_U:
				case Code.Conv_Ovf_U:
				case Code.Conv_Ovf_U_Un:
					Conv_U(ctx, instruction);
					break;
				case Code.Conv_U1:
				case Code.Conv_Ovf_U1:
				case Code.Conv_Ovf_U1_Un:
					Conv_U1(ctx, instruction);
					break;
				case Code.Conv_U2:
				case Code.Conv_Ovf_U2:
				case Code.Conv_Ovf_U2_Un:
					Conv_U2(ctx, instruction);
					break;
				case Code.Conv_U4:
				case Code.Conv_Ovf_U4:
				case Code.Conv_Ovf_U4_Un:
					Conv_U4(ctx, instruction);
					break;
				case Code.Conv_U8:
				case Code.Conv_Ovf_U8:
				case Code.Conv_Ovf_U8_Un:
					Conv_U8(ctx, instruction);
					break;
				case Code.Cpblk:
					Cpblk(ctx, instruction);
					break;
				case Code.Cpobj:
					Cpobj(ctx, instruction);
					break;
				case Code.Div:
				case Code.Div_Un:
					Div(ctx, instruction);
					break;
				case Code.Dup:
					ctx.Stack.Push(ctx.Stack.Peek());
					break;
				case Code.Endfilter:
				case Code.Endfinally:
					break;
				case Code.Initblk:
					Initblk(ctx, instruction);
					break;
				case Code.Initobj:
					Initobj(ctx, instruction);
					break;
				case Code.Isinst:
					Isinst(ctx, instruction);
					break;
				case Code.Jmp:
					break;
				case Code.Ldarg:
				case Code.Ldarg_S:
					Ldarg(ctx, instruction);
					break;
				case Code.Ldarg_0:
					Ldarg(ctx, instruction, 0);
					break;
				case Code.Ldarg_1:
					Ldarg(ctx, instruction, 1);
					break;
				case Code.Ldarg_2:
					Ldarg(ctx, instruction, 2);
					break;
				case Code.Ldarg_3:
					Ldarg(ctx, instruction, 3);
					break;
				case Code.Ldarga:
				case Code.Ldarga_S:
					Ldarga(ctx, instruction);
					break;
				case Code.Ldc_I4:
					Ldc_I4(ctx, instruction, (int)instruction.Operand);
					break;
				case Code.Ldc_I4_0:
					Ldc_I4(ctx, instruction, 0);
					break;
				case Code.Ldc_I4_1:
					Ldc_I4(ctx, instruction, 1);
					break;
				case Code.Ldc_I4_2:
					Ldc_I4(ctx, instruction, 2);
					break;
				case Code.Ldc_I4_3:
					Ldc_I4(ctx, instruction, 3);
					break;
				case Code.Ldc_I4_4:
					Ldc_I4(ctx, instruction, 4);
					break;
				case Code.Ldc_I4_5:
					Ldc_I4(ctx, instruction, 5);
					break;
				case Code.Ldc_I4_6:
					Ldc_I4(ctx, instruction, 6);
					break;
				case Code.Ldc_I4_7:
					Ldc_I4(ctx, instruction, 7);
					break;
				case Code.Ldc_I4_8:
					Ldc_I4(ctx, instruction, 8);
					break;
				case Code.Ldc_I4_M1:
					Ldc_I4(ctx, instruction, -1);
					break;
				case Code.Ldc_I4_S:
					Ldc_I4(ctx, instruction, (sbyte)instruction.Operand);
					break;
				case Code.Ldc_I8:
					Ldc_I8(ctx, instruction, (long)instruction.Operand);
					break;
				case Code.Ldc_R4:
					Ldc_R4(ctx, instruction, (float)instruction.Operand);
					break;
				case Code.Ldc_R8:
					Ldc_R8(ctx, instruction, (double)instruction.Operand);
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
					Ldelem(ctx, instruction);
					break;
				case Code.Ldelema:
					Ldelema(ctx, instruction);
					break;
				case Code.Ldfld:
				case Code.Ldflda:
				case Code.Ldsfld:
				case Code.Ldsflda:
					Ldfld(ctx, instruction);
					break;
				case Code.Ldftn:
					Ldftn(ctx, instruction);
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
					Ldind(ctx, instruction);
					break;
				case Code.Ldlen:
					Ldlen(ctx, instruction);
					break;
				case Code.Ldloc:
				case Code.Ldloc_S:
					Ldloc(ctx, instruction);
					break;
				case Code.Ldloc_0:
					Ldloc(ctx, instruction, 0);
					break;
				case Code.Ldloc_1:
					Ldloc(ctx, instruction, 1);
					break;
				case Code.Ldloc_2:
					Ldloc(ctx, instruction, 2);
					break;
				case Code.Ldloc_3:
					Ldloc(ctx, instruction, 3);
					break;
				case Code.Ldloca:
				case Code.Ldloca_S:
					Ldloca(ctx, instruction);
					break;
				case Code.Ldnull:
					ctx.Stack.Push(Cil.Null.Instance);
					break;
				case Code.Ldobj:
					Ldind(ctx, instruction);
					break;
				case Code.Ldstr:
					Ldstr(ctx, instruction);
					break;
				case Code.Ldtoken:
					Ldtoken(ctx, instruction);
					break;
				case Code.Ldvirtftn:
					Ldvirtftn(ctx, instruction);
					break;
				case Code.Leave:
				case Code.Leave_S:
					nextInstruction = (Instruction)instruction.Operand;
					break;
				case Code.Localloc:
					Localloc(ctx, instruction);
					break;
				case Code.Mkrefany:
					Mkref(ctx, instruction);
					break;
				case Code.Mul:
				case Code.Mul_Ovf:
				case Code.Mul_Ovf_Un:
					Mul(ctx, instruction);
					break;
				case Code.Neg:
					Neg(ctx, instruction);
					break;
				case Code.Newarr:
					Newarr(ctx, instruction);
					break;
				case Code.Newobj:
					Newobj(ctx, instruction);
					break;
				case Code.Nop:
					break;
				case Code.Not:
					Not(ctx, instruction);
					break;
				case Code.Or:
					Or(ctx, instruction);
					break;
				case Code.Pop:
					ctx.Stack.Pop();
					break;
				case Code.Readonly:
					break;
				case Code.Refanytype:
					Refanytype(ctx, instruction);
					break;
				case Code.Refanyval:
					Refanyval(ctx, instruction);
					break;
				case Code.Rem:
				case Code.Rem_Un:
					Rem(ctx, instruction);
					break;
				case Code.Ret:
				case Code.Rethrow:
					nextInstruction = null;
					break;
				case Code.Shl:
					Shl(ctx, instruction);
					break;
				case Code.Shr:
				case Code.Shr_Un:
					Shr(ctx, instruction);
					break;
				case Code.Sizeof:
					Sizeof(ctx, instruction);
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
					Stfld(ctx, instruction);
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
					Sub(ctx, instruction);
					break;
				case Code.Switch:
					Switch(ctx, branches, instruction, out nextInstruction);
					break;
				case Code.Tail:
					break;
				case Code.Throw:
					ctx.Stack.Pop();
					nextInstruction = null;
					break;
				case Code.Unaligned:
					Unaligned(ctx, instruction);
					break;
				case Code.Unbox:
				case Code.Unbox_Any:
					Unbox(ctx, instruction);
					break;
				case Code.Volatile:
					break;
				case Code.Xor:
					Xor(ctx, instruction);
					break;
				default: throw new NotImplementedException($"Unknown instruction {instruction.OpCode.Code}");
			}
		}

		delegate Ast ConditionProvider(CilPoint pt, Stack<Ast> stack);
		delegate Ast UnaryConditionProvider(CilPoint pt, Ast value);
		delegate Ast BinaryConditionProvider(CilPoint pt, Ast left, Ast right);

		private static void Branch(ConditionProvider prov, RoutineContext ctx, List<RoutineContext> branches, Instruction instruction, out Instruction nextInstruction)
		{
			nextInstruction = (Instruction)instruction.Operand;

			var pt = GetCilPoint(ctx, instruction);
			var conditionAst = prov(pt, ctx.Stack);

			var altBranch = new RoutineContext(ctx, instruction.Next);
			altBranch.Conditions.Add(new Condition(conditionAst, 0));
			branches.Add(altBranch);

			ctx.Conditions.Add(new Condition(conditionAst, 1));
		}

		private static void Branch(UnaryConditionProvider prov, RoutineContext ctx, List<RoutineContext> branches, Instruction instruction, out Instruction nextInstruction)
		{
			Branch((CilPoint pt, Stack<Ast> stack) =>
			{
				var value = stack.Pop();
				return prov(pt, value);
			},
			ctx, branches, instruction, out nextInstruction);
		}

		private static void Branch(BinaryConditionProvider prov, RoutineContext ctx, List<RoutineContext> branches, Instruction instruction, out Instruction nextInstruction)
		{
			Branch((CilPoint pt, Stack<Ast> stack) =>
			{
				var right = ctx.Stack.Pop();
				var left = ctx.Stack.Pop();
				return prov(pt, left, right);
			},
			ctx, branches, instruction, out nextInstruction);
		}

		private static void Switch(RoutineContext ctx, List<RoutineContext> branches, Instruction instruction, out Instruction nextInstruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			var conditionAst = new Cil.Switch(pt, value);

			var tbl = (Instruction[])instruction.Operand;
			for (var i = 1; i < tbl.Length; ++i)
			{
				var nthBranch = new RoutineContext(ctx, tbl[i]);
				nthBranch.Conditions.Add(new Condition(conditionAst, i));
				branches.Add(nthBranch);
			}

			ctx.Conditions.Add(new Condition(conditionAst, 0));
			nextInstruction = tbl[0];
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

		private static CilPoint GetCilPoint(RoutineContext ctx, Instruction instruction)
		{
			SequencePoint sp = null;
			if (ctx.SequencePoints != null && ctx.SequencePoints.Length > 0)
				sp = ctx.SequencePoints.Where(x => x.Offset <= instruction.Offset).LastOrDefault();

			return new CilPoint(ctx.Method, instruction.Offset, sp);
		}

		private static void Add(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Add(pt, left, right));
		}

		private static void And(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.And(pt, left, right));
		}

		private static void Arglist(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			ctx.Stack.Push(new Cil.Arglist(pt, ctx.Method));
		}

		private static void Box(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Box(pt, value));
		}

		private static void Call(RoutineContext ctx, Instruction instruction, ICilMachineContext machineContext)
		{
			var method = (MethodReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			var args = PopArgs(ctx, method);

			Ast instance = null;
			if (method.HasThis)
				instance = ctx.Stack.Pop();

			var methodFullName = machineContext?.GetMethodFullName(method);
			var call = new Cil.Call(pt, instance, method, args, methodFullName);

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
					ctx.Vars[var.Index] = new Cil.OutArg(pt, call, i);
			}
		}

		private static void Castclass(RoutineContext ctx, Instruction instruction)
		{
			var type = (TypeReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Castclass(pt, type, value));
		}

		private static void Ceq(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Ceq(pt, left, right));
		}

		private static void Cgt(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Cgt(pt, left, right));
		}

		private static void Clt(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Clt(pt, left, right));
		}

		private static void Conv_I(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_I(pt, value));
		}

		private static void Conv_I1(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_I1(pt, value));
		}

		private static void Conv_I2(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_I2(pt, value));
		}

		private static void Conv_I4(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_I4(pt, value));
		}

		private static void Conv_I8(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_I8(pt, value));
		}

		private static void Conv_R4(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_R4(pt, value));
		}

		private static void Conv_R8(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_R8(pt, value));
		}

		private static void Conv_U(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_U(pt, value));
		}

		private static void Conv_U1(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_U1(pt, value));
		}

		private static void Conv_U2(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_U2(pt, value));
		}

		private static void Conv_U4(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_U4(pt, value));
		}

		private static void Conv_U8(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Conv_U8(pt, value));
		}

		private static void Cpblk(RoutineContext ctx, Instruction instruction)
		{
			var count = ctx.Stack.Pop();
			var src = ctx.Stack.Pop();
			var dest = ctx.Stack.Pop();
		}

		private static void Cpobj(RoutineContext ctx, Instruction instruction)
		{
			var src = ctx.Stack.Pop();
			var dest = ctx.Stack.Pop();
		}

		private static void Div(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Div(pt, left, right));
		}

		private static void Initblk(RoutineContext ctx, Instruction instruction)
		{
			var count = ctx.Stack.Pop();
			var value = ctx.Stack.Pop();
			var address = ctx.Stack.Pop();
		}

		private static void Initobj(RoutineContext ctx, Instruction instruction)
		{
			var address = ctx.Stack.Pop();
		}

		public static void Isinst(RoutineContext ctx, Instruction instruction)
		{
			var type = (TypeReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			var instance = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Isinst(pt, type, instance));
		}

		private static void Ldarg(RoutineContext ctx, Instruction instruction)
		{
			var number = ((ParameterReference)instruction.Operand).Index;
			Ldarg(ctx, instruction, number);
		}

		private static void Ldarg(RoutineContext ctx, Instruction instruction, int number)
		{
			var pt = GetCilPoint(ctx, instruction);

			if (ctx.Method.HasThis && number == 0)
			{
				ctx.Stack.Push(new Cil.This(pt, ctx.Method.DeclaringType));
				return;
			}

			var argNum = ctx.Method.HasThis ? number - 1 : number;
			var value = ctx.Args[argNum];
			if (value == null)
			{
				var parameter = ctx.Method.Parameters[argNum];
				value = new Cil.Arg(pt, argNum, parameter);
			}
			ctx.Stack.Push(value);
		}

		private static void Ldarga(RoutineContext ctx, Instruction instruction)
		{
			var number = ((ParameterReference)instruction.Operand).Index;
			var pt = GetCilPoint(ctx, instruction);
			ctx.Stack.Push(new Cil.Argptr(pt, number));
		}

		private static void Ldc_I4(RoutineContext ctx, Instruction instruction, int value)
		{
			var pt = GetCilPoint(ctx, instruction);
			ctx.Stack.Push(new Cil.Int32(pt, value));
		}

		private static void Ldc_I8(RoutineContext ctx, Instruction instruction, long value)
		{
			var pt = GetCilPoint(ctx, instruction);
			ctx.Stack.Push(new Cil.Int64(pt, value));
		}

		private static void Ldc_R4(RoutineContext ctx, Instruction instruction, float value)
		{
			var pt = GetCilPoint(ctx, instruction);
			ctx.Stack.Push(new Cil.Float32(pt, value));
		}

		private static void Ldc_R8(RoutineContext ctx, Instruction instruction, double value)
		{
			var pt = GetCilPoint(ctx, instruction);
			ctx.Stack.Push(new Cil.Float64(pt, value));
		}

		private static void Ldelem(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var index = ctx.Stack.Pop();
			var array = ctx.Stack.Pop();

			if (ctx.Arrays.TryGetValue(new ArrayIndex(array, index), out var value) == false)
				value = new Cil.Elem(pt, array, index);

			ctx.Stack.Push(value);
		}

		private static void Ldelema(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var index = ctx.Stack.Pop();
			var array = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Elemptr(pt, array, index));
		}

		private static void Ldfld(RoutineContext ctx, Instruction instruction)
		{
			var fld = (FieldReference)instruction.Operand;
			var fldImpl = fld.Resolve();
			var pt = GetCilPoint(ctx, instruction);

			Ast instance = null;
			if (fldImpl != null && fldImpl.IsStatic == false)
				instance = ctx.Stack.Pop();

			if (ctx.Objects.TryGetValue(new ObjectField(instance, fld), out var value) == false)
				value = new Cil.Fld(pt, instance, fld);

			ctx.Stack.Push(value);
		}

		private static void Ldftn(RoutineContext ctx, Instruction instruction)
		{
			var mtd = (MethodReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			ctx.Stack.Push(new Cil.Ftn(pt, null, mtd));
		}

		private static void Ldind(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var address = ctx.Stack.Pop();
			if (ctx.Heap.TryGetValue(address, out var value) == false)
				value = new Cil.Heapptr(pt, address);
			ctx.Stack.Push(value);
		}

		private static void Ldlen(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var array = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Len(pt, array));
		}

		private static void Ldloc(RoutineContext ctx, Instruction instruction)
		{
			var number = ((VariableReference)instruction.Operand).Index;
			Ldloc(ctx, instruction, number);
		}

		private static void Ldloc(RoutineContext ctx, Instruction instruction, int number)
		{
			ctx.Stack.Push(ctx.Vars[number]);
		}

		private static void Ldloca(RoutineContext ctx, Instruction instruction)
		{
			var v = (VariableReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);

			var value = ctx.Vars[v.Index];
			if (value != null)
				ctx.Stack.Push(value);
			else
				ctx.Stack.Push(new Cil.Varptr(pt, v.Index));
		}

		private static void Ldstr(RoutineContext ctx, Instruction instruction)
		{
			var value = (string)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			ctx.Stack.Push(new Cil.String(pt, value));
		}

		private static void Ldtoken(RoutineContext ctx, Instruction instruction)
		{
			var value = instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);

			Ast token;
			if (value is TypeReference t)
				token = new Cil.Typeof(pt, t);
			else if (value is MethodReference m)
				token = new Cil.Methodof(pt, m);
			else if (value is FieldReference f)
				token = new Cil.Fieldof(pt, f);
			else throw new NotImplementedException($"Unknown token {value}");

			ctx.Stack.Push(token);
		}

		private static void Ldvirtftn(RoutineContext ctx, Instruction instruction)
		{
			var mtd = (MethodReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			var instance = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Ftn(pt, instance, mtd));
		}

		private static void Localloc(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var count = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Bytes(pt, count));
		}

		private static void Mkref(RoutineContext ctx, Instruction instruction)
		{
			var type = (TypeReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			var address = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Mkref(pt, address, type));
		}

		private static void Mul(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Mul(pt, left, right));
		}

		private static void Neg(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Neg(pt, value));
		}

		private static void Newarr(RoutineContext ctx, Instruction instruction)
		{
			var type = (TypeReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			var size = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Array(pt, type, size));
		}

		private static void Newobj(RoutineContext ctx, Instruction instruction)
		{
			var ctor = (MethodReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			var args = PopArgs(ctx, ctor);
			var newobj = new Cil.Newobj(pt, ctor.DeclaringType, ctor, args);

			ctx.Expressions.RemoveAll(args);
			ctx.Expressions.Add(newobj);

			ctx.Stack.Push(newobj);
		}

		private static void Not(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Not(pt, value));
		}

		private static void Or(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Or(pt, left, right));
		}

		private static void Refanytype(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var reference = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Refanytype(pt, reference));
		}

		private static void Refanyval(RoutineContext ctx, Instruction instruction)
		{
			var type = (TypeReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			var address = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Refanyval(pt, type, address));
		}

		private static void Rem(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Rem(pt, left, right));
		}

		private static void Shl(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var count = ctx.Stack.Pop();
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Shl(pt, value, count));
		}

		private static void Shr(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var count = ctx.Stack.Pop();
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Shr(pt, value, count));
		}

		private static void Sizeof(RoutineContext ctx, Instruction instruction)
		{
			var type = (TypeReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			ctx.Stack.Push(new Cil.Sizeof(pt, type));
		}

		private static void Starg(RoutineContext ctx, int number)
		{
			var value = ctx.Stack.Pop();
			ctx.Args[number] = value;
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

		private static void Stfld(RoutineContext ctx, Instruction instruction)
		{
			var fld = (FieldReference)instruction.Operand;
			var fldImpl = fld.Resolve();
			var value = ctx.Stack.Pop();

			Ast instance = null;
			if (fldImpl != null && fldImpl.IsStatic == false)
				instance = ctx.Stack.Pop();

			ctx.Objects.AddOrReplace(new ObjectField(instance, fld), value);
		}

		private static void Sub(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Sub(pt, left, right));
		}

		private static void Unaligned(RoutineContext ctx, Instruction instruction)
		{
			var address = (byte)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			ctx.Stack.Push(new Cil.Unaligned(pt, address));
		}

		private static void Unbox(RoutineContext ctx, Instruction instruction)
		{
			var type = (TypeReference)instruction.Operand;
			var pt = GetCilPoint(ctx, instruction);
			var value = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Unbox(pt, type, value));
		}

		private static void Xor(RoutineContext ctx, Instruction instruction)
		{
			var pt = GetCilPoint(ctx, instruction);
			var right = ctx.Stack.Pop();
			var left = ctx.Stack.Pop();
			ctx.Stack.Push(new Cil.Xor(pt, left, right));
		}
		#endregion
	}
	#endregion
}
