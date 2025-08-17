using Flint.Common;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm
{
	static class CilMachine
	{
		#region Interface
		public static List<Ast> Evaluate(MethodDefinition mtd)
		{
			var ctx = new RoutineContext(mtd);
			foreach (var instruction in mtd.Body.Instructions)
			{
				switch (instruction.OpCode.Code)
				{
					case Code.Nop: break;
					case Code.Ret: break;
					case Code.Call:
						Call(ctx, (MethodReference)instruction.Operand);
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
			return ctx.Expressions.ToList();
		}
		#endregion

		#region Implementation
		class RoutineContext
		{
			public readonly HashSet<Ast> Expressions;
			public readonly Ast[] Variables;
			public readonly Stack<Ast> Stack;
			public RoutineContext(MethodDefinition mtd)
			{
				Expressions = [];
				Variables = new Ast[mtd.Body.Variables.Count];
				Stack = new Stack<Ast>(mtd.Body.MaxStackSize);
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


		#endregion
	}
}
