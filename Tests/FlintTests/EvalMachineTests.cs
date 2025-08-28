using Flint.Vm;
using FluentAssertions;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Cil = Flint.Vm.Cil;

namespace FlintTests
{
	[TestClass]
	public class EvalMachineTests
	{
		private static readonly TypeDefinition IntT = CecilType<int>();
		private static readonly TypeDefinition StringT = CecilType<string>();

		private static TypeDefinition CecilType<T>()
		{
			var t = typeof(T);
			return new TypeDefinition(t.Namespace, t.Name, 0);
		}

		private static bool IsEmpty(EvalMachine.RoutineContext ctx)
		{
			return ctx.Memory.Count == 0
				&& ctx.Variables.All(x => x is null)
				&& ctx.Stack.Count == 0
				&& ctx.Expressions.Count == 0;
		}

		[TestMethod]
		public void Add()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Add);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().BeEquivalentTo(new Cil.Add(new Cil.Int32(1), new Cil.Int32(2)));
		}

		[TestMethod]
		public void Add_Ovf()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Add_Ovf);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().BeEquivalentTo(new Cil.Add(new Cil.Int32(1), new Cil.Int32(2)));
		}

		[TestMethod]
		public void Add_Ovf_Un()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Add_Ovf_Un);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().BeEquivalentTo(new Cil.Add(new Cil.Int32(1), new Cil.Int32(2)));
		}

		[TestMethod]
		public void And()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.And);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().BeEquivalentTo(new Cil.And(new Cil.Int32(1), new Cil.Int32(2)));
		}

		[TestMethod]
		public void Arglist()
		{
			var method = new MethodDefinition("test", 0, IntT);
			var ctx = new EvalMachine.RoutineContext(method, 1, 2);
			var instruction = Instruction.Create(OpCodes.Arglist);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().BeEquivalentTo(new Cil.Arglist(method));
		}

		[TestMethod]
		public void Beq()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Beq, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Beq_S()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Beq_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Bge()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Bge, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Bge_S()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Bge_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Bge_Un()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Bge_Un, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Bge_Un_S()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Bge_Un_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Bgt()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Bgt, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Bgt_S()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Bgt_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Bgt_Un()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Bgt_Un, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Bgt_Un_s()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Bgt_Un_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Ble()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Ble, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Ble_S()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Ble_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Ble_Un()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Ble_Un, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Ble_Un_S()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Ble_Un_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Blt()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Blt, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Blt_S()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Blt_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Blt_Un()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Blt_Un, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Blt_Un_S()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Blt_Un_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Bne_Un()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Bne_Un, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Bne_Un_S()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Bne_Un_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Box()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(42));
			var instruction = Instruction.Create(OpCodes.Box, IntT);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().BeEquivalentTo(new Cil.Box(new Cil.Int32(42)));
		}

		[TestMethod]
		public void Br()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			var instruction = Instruction.Create(OpCodes.Br, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			IsEmpty(ctx).Should().BeTrue();
		}

		[TestMethod]
		public void Br_S()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			var instruction = Instruction.Create(OpCodes.Br_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			IsEmpty(ctx).Should().BeTrue();
		}

		[TestMethod]
		public void Break()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			var instruction = Instruction.Create(OpCodes.Break);

			EvalMachine.Eval(ctx, instruction);

			IsEmpty(ctx).Should().BeTrue();
		}









		[TestMethod]
		public void Isinst()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Isinst, StringT);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().BeEquivalentTo(new Cil.IsInstance(StringT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Nop()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			var instruction = Instruction.Create(OpCodes.Nop);

			EvalMachine.Eval(ctx, instruction);

			IsEmpty(ctx).Should().BeTrue();
		}

		[TestMethod]
		public void Throw()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Exception.Instance);
			var instruction = Instruction.Create(OpCodes.Throw);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

	}
}
