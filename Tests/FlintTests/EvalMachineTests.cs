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
		readonly int IntField = default;
		readonly string StringField = default;
		class Class
		{
			public static bool StaticMethod(int x, string s) => false;
			public bool InstanceMethod(int x, string s) => false;
			public void VoidMethod(int x, string s) { }
			public virtual bool VirtualMethod(int x, string s) => false;
			public void ManyParametersMethod(int x1, int x2, int x3, int x4, int x5) { }
		}

		private static readonly TypeDefinition IntT;
		private static readonly TypeDefinition StringT;
		private static readonly TypeDefinition ClassT;
		private static readonly MethodDefinition StaticMethodT;
		private static readonly MethodDefinition InstanceMethodT;
		private static readonly MethodDefinition VoidMethodT;
		private static readonly MethodDefinition VirtualMethodT;
		private static readonly MethodDefinition ManyParametersMethodT;

		static EvalMachineTests()
		{
			using var tests = ModuleDefinition.ReadModule("FlintTests.dll");
			var @this = tests.GetTypes().Where(t => t.FullName == "FlintTests.EvalMachineTests").First();

			IntT = @this.Fields.Where(f => f.Name == nameof(IntField)).Select(f => f.FieldType).First().Resolve();
			StringT = @this.Fields.Where(f => f.Name == nameof(StringField)).Select(f => f.FieldType).First().Resolve();
			ClassT = tests.GetTypes().Where(t => t.FullName == "FlintTests.EvalMachineTests/Class").First();
			StaticMethodT = ClassT.Methods.Where(m => m.Name == nameof(Class.StaticMethod)).First();
			InstanceMethodT = ClassT.Methods.Where(m => m.Name == nameof(Class.InstanceMethod)).First();
			VoidMethodT = ClassT.Methods.Where(m => m.Name == nameof(Class.VoidMethod)).First();
			VirtualMethodT = ClassT.Methods.Where(m => m.Name == nameof(Class.VirtualMethod)).First();
			ManyParametersMethodT = ClassT.Methods.Where(m => m.Name == nameof(Class.ManyParametersMethod)).First();
		}

		private static bool IsEmpty(EvalMachine.RoutineContext ctx)
		{
			return ctx.Memory.Count == 0
				&& ctx.Variables.All(x => x is null)
				&& ctx.Stack.Count == 0
				&& ctx.Expressions.Count == 0;
		}

		private static ParameterDefinition Param(int index)
		{
			var p = new ParameterDefinition(IntT);
			typeof(ParameterDefinition)
				.GetField("index", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField)
				.SetValue(p, index);
			return p;
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
			ctx.Stack.Peek().Should().Be(new Cil.Add(new Cil.Int32(1), new Cil.Int32(2)));
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
			ctx.Stack.Peek().Should().Be(new Cil.Add(new Cil.Int32(1), new Cil.Int32(2)));
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
			ctx.Stack.Peek().Should().Be(new Cil.Add(new Cil.Int32(1), new Cil.Int32(2)));
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
			ctx.Stack.Peek().Should().Be(new Cil.And(new Cil.Int32(1), new Cil.Int32(2)));
		}

		[TestMethod]
		public void Arglist()
		{
			var method = new MethodDefinition("test", 0, IntT);
			var ctx = new EvalMachine.RoutineContext(method, 1, 2);
			var instruction = Instruction.Create(OpCodes.Arglist);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Arglist(method));
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
			ctx.Stack.Peek().Should().Be(new Cil.Box(new Cil.Int32(42)));
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
		public void Brfalse()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(0));
			var instruction = Instruction.Create(OpCodes.Brfalse, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Brfalse_S()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(0));
			var instruction = Instruction.Create(OpCodes.Brfalse_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Brtrue()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(0));
			var instruction = Instruction.Create(OpCodes.Brtrue, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Brtrue_S()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(0));
			var instruction = Instruction.Create(OpCodes.Brtrue_S, Instruction.Create(OpCodes.Nop));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Call_StaticMethod()
		{
			var ctx = new EvalMachine.RoutineContext(0, 2);
			ctx.Stack.Push(new Cil.Int32(42));
			ctx.Stack.Push(new Cil.String("foo"));
			var instruction = Instruction.Create(OpCodes.Call, StaticMethodT);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(
				new Cil.Call(null, StaticMethodT, [new Cil.Int32(42), new Cil.String("foo")]));
		}

		[TestMethod]
		public void Call_InstanceMethod()
		{
			var ctx = new EvalMachine.RoutineContext(0, 3);
			ctx.Stack.Push(new Cil.This(ClassT));
			ctx.Stack.Push(new Cil.Int32(42));
			ctx.Stack.Push(new Cil.String("foo"));
			var instruction = Instruction.Create(OpCodes.Call, InstanceMethodT);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(
				new Cil.Call(new Cil.This(ClassT), InstanceMethodT, [new Cil.Int32(42), new Cil.String("foo")]));
		}

		[TestMethod]
		public void Call_VoidMethod()
		{
			var ctx = new EvalMachine.RoutineContext(0, 3);
			ctx.Stack.Push(new Cil.This(ClassT));
			ctx.Stack.Push(new Cil.Int32(42));
			ctx.Stack.Push(new Cil.String("foo"));
			var instruction = Instruction.Create(OpCodes.Call, VoidMethodT);

			EvalMachine.Eval(ctx, instruction);

			// do not push a result on stack if method has void result type
			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Callvirt()
		{
			var ctx = new EvalMachine.RoutineContext(0, 3);
			ctx.Stack.Push(new Cil.This(ClassT));
			ctx.Stack.Push(new Cil.Int32(42));
			ctx.Stack.Push(new Cil.String("foo"));
			var instruction = Instruction.Create(OpCodes.Callvirt, VirtualMethodT);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(
				new Cil.Call(new Cil.This(ClassT), VirtualMethodT, [new Cil.Int32(42), new Cil.String("foo")]));
		}

		[TestMethod]
		public void Castclass()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(42));
			var instruction = Instruction.Create(OpCodes.Castclass, ClassT);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Castclass(ClassT, new Cil.Int32(42)));
		}

		[TestMethod]
		public void Ceq()
		{
			var ctx = new EvalMachine.RoutineContext(0, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Ceq);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Ceq(new Cil.Int32(1), new Cil.Int32(2)));
		}

		[TestMethod]
		public void Cgt()
		{
			var ctx = new EvalMachine.RoutineContext(0, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Cgt);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Cgt(new Cil.Int32(1), new Cil.Int32(2)));
		}

		[TestMethod]
		public void Cgt_Un()
		{
			var ctx = new EvalMachine.RoutineContext(0, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Cgt_Un);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Cgt(new Cil.Int32(1), new Cil.Int32(2)));
		}

		[TestMethod]
		public void Ckfinite()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(new Cil.Int32(42));
			var instruction = Instruction.Create(OpCodes.Ckfinite);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Int32(42));
		}

		[TestMethod]
		public void Clt()
		{
			var ctx = new EvalMachine.RoutineContext(0, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Clt);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Clt(new Cil.Int32(1), new Cil.Int32(2)));
		}

		[TestMethod]
		public void Clt_Un()
		{
			var ctx = new EvalMachine.RoutineContext(0, 2);
			ctx.Stack.Push(new Cil.Int32(1));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Clt_Un);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Clt(new Cil.Int32(1), new Cil.Int32(2)));
		}

		[TestMethod]
		public void Constrained()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			var instruction = Instruction.Create(OpCodes.Constrained, ClassT);

			EvalMachine.Eval(ctx, instruction);

			IsEmpty(ctx).Should().BeTrue();
		}

		[TestMethod]
		public void Conv_I()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_I);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_I1()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_I1);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I1(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_I2()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_I2);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I2(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_I4()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_I4);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I4(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_I8()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_I8);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I8(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I_Un()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I_Un);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I1()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I1);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I1(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I1_Un()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I1_Un);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I1(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I2()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I2);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I2(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I2_Un()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I2_Un);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I2(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I4()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I4);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I4(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I4_Un()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I4_Un);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I4(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I8()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I8);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I8(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I8_Un()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I8_Un);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_I8(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_R_Un()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_R_Un);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_R4(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_R4()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_R4);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_R4(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_R8()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_R8);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_R8(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_U()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_U);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_U(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_U1()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_U1);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_U1(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_U2()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_U2);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_U2(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_U4()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_U4);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_U4(Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_U8()
		{
			var ctx = new EvalMachine.RoutineContext(0, 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_U8);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Conv_U8(Cil.Null.Instance));
		}

		[TestMethod]
		public void Cpblk()
		{
			var ctx = new EvalMachine.RoutineContext(0, 3);
			ctx.Stack.Push(new Cil.Int32(2000)); // destination address
			ctx.Stack.Push(new Cil.Int32(1000)); // source address
			ctx.Stack.Push(new Cil.Int32(10)); // number of bytes to copy
			var instruction = Instruction.Create(OpCodes.Cpblk);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Cpobj()
		{
			var ctx = new EvalMachine.RoutineContext(0, 2);
			ctx.Stack.Push(new Cil.Int32(2000)); // destination object address
			ctx.Stack.Push(new Cil.Int32(1000)); // source object address
			var instruction = Instruction.Create(OpCodes.Cpobj, ClassT);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Div()
		{
			var ctx = new EvalMachine.RoutineContext(0, 2);
			ctx.Stack.Push(new Cil.Int32(10));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Div);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(
				new Cil.Div(new Cil.Int32(10), new Cil.Int32(2)));
		}

		[TestMethod]
		public void Div_Un()
		{
			var ctx = new EvalMachine.RoutineContext(0, 2);
			ctx.Stack.Push(new Cil.Int32(10));
			ctx.Stack.Push(new Cil.Int32(2));
			var instruction = Instruction.Create(OpCodes.Div_Un);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(
				new Cil.Div(new Cil.Int32(10), new Cil.Int32(2)));
		}

		[TestMethod]
		public void Dup()
		{
			var ctx = new EvalMachine.RoutineContext(0, 2);
			ctx.Stack.Push(new Cil.Int32(42));
			var instruction = Instruction.Create(OpCodes.Dup);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(2);
			ctx.Stack.Pop().Should().Be(new Cil.Int32(42));
			ctx.Stack.Pop().Should().Be(new Cil.Int32(42));
		}

		[TestMethod]
		public void Endfilter()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			var instruction = Instruction.Create(OpCodes.Endfilter);

			EvalMachine.Eval(ctx, instruction);

			IsEmpty(ctx).Should().BeTrue();
		}

		[TestMethod]
		public void Endfinally()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			var instruction = Instruction.Create(OpCodes.Endfinally);

			EvalMachine.Eval(ctx, instruction);

			IsEmpty(ctx).Should().BeTrue();
		}

		[TestMethod]
		public void Initblk()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1000)); // starting address
			ctx.Stack.Push(new Cil.Int32(42)); // initialization value
			ctx.Stack.Push(new Cil.Int32(10)); // number of bytes to initialize
			var instruction = Instruction.Create(OpCodes.Initblk);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Initobj()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.Int32(1000)); // address
			var instruction = Instruction.Create(OpCodes.Initobj, ClassT);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().BeEmpty();
		}

		[TestMethod]
		public void Isinst()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			ctx.Stack.Push(new Cil.String("foo"));
			var instruction = Instruction.Create(OpCodes.Isinst, StringT);

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(
				new Cil.Isinst(StringT, new Cil.String("foo")));
		}

		[TestMethod]
		public void Jmp()
		{
			var method = ModuleDefinition.ReadModule("FlintTests.dll")
				.GetTypes().Where(t => t.FullName == "FlintTests.EvalMachineTests/Class").First()
				.Methods.Where(m => m.Name == "VoidMethod").First();

			var ctx = new EvalMachine.RoutineContext(0, 1);
			var instruction = Instruction.Create(OpCodes.Jmp, method);

			EvalMachine.Eval(ctx, instruction);

			IsEmpty(ctx).Should().BeTrue();
		}

		[TestMethod]
		public void Ldarg_InstanceMethod()
		{
			var ctx = new EvalMachine.RoutineContext(InstanceMethodT, 0, 1);
			var instruction = Instruction.Create(OpCodes.Ldarg, Param(1));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Arg(0, InstanceMethodT.Parameters[0]));
		}

		[TestMethod]
		public void Ldarg_StaticMethod()
		{
			var ctx = new EvalMachine.RoutineContext(StaticMethodT, 0, 1);
			var instruction = Instruction.Create(OpCodes.Ldarg, Param(1));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.Arg(1, StaticMethodT.Parameters[1]));
		}

		[TestMethod]
		public void Ldarg_This()
		{
			var ctx = new EvalMachine.RoutineContext(InstanceMethodT, 0, 1);
			var instruction = Instruction.Create(OpCodes.Ldarg, Param(0));

			EvalMachine.Eval(ctx, instruction);

			ctx.Stack.Should().HaveCount(1);
			ctx.Stack.Peek().Should().Be(new Cil.This(ClassT));
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
