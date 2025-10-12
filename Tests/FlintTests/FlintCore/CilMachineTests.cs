using Flint.Vm;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Cil = Flint.Vm.Cil;

#pragma warning disable CS0618
namespace FlintTests.FlintCore
{
	[TestClass]
	public class CilMachineTests
	{
		static readonly SequencePoint SP = new SequencePoint(Instruction.Create(OpCodes.Nop), new Document("test"));
		static readonly CilPoint PT = new CilPoint(null, 0, SP);

		readonly int IntField = default;
		readonly static string StringField = default;
		class Class
		{
			public Class(int x, string s) { }
			public static bool StaticMethod(int x, string s) => false;
			public bool InstanceMethod(int x, string s) => false;
			public void VoidMethod(int x, string s) { }
			public virtual bool VirtualMethod(int x, string s) => false;
			public static void ManyParametersMethod(int x1, int x2, int x3, int x4, int x5) { }
			public static int[] ArrayMethod() => null;
		}

		private static readonly FieldDefinition IntFieldT;
		private static readonly FieldDefinition StringFieldT;
		private static readonly TypeDefinition IntT;
		private static readonly TypeDefinition StringT;
		private static readonly TypeDefinition ClassT;
		private static readonly MethodDefinition ConstructorT;
		private static readonly MethodDefinition StaticMethodT;
		private static readonly MethodDefinition InstanceMethodT;
		private static readonly MethodDefinition VoidMethodT;
		private static readonly MethodDefinition VirtualMethodT;
		private static readonly MethodDefinition ManyParametersMethodT;
		private static readonly MethodDefinition ArrayMethodT;

		static CilMachineTests()
		{
			using var tests = ModuleDefinition.ReadModule("FlintTests.dll");
			var @this = tests.GetTypes().Where(t => t.Name == nameof(CilMachineTests)).First();

			IntFieldT = @this.Fields.Where(f => f.Name == nameof(IntField)).First();
			StringFieldT = @this.Fields.Where(f => f.Name == nameof(StringField)).First();
			IntT = IntFieldT.FieldType.Resolve();
			StringT = StringFieldT.FieldType.Resolve();
			ClassT = tests.GetTypes().Where(t => t.Name == nameof(Class)).First();
			ConstructorT = ClassT.Methods.Where(m => m.Name == ".ctor").First();
			StaticMethodT = ClassT.Methods.Where(m => m.Name == nameof(Class.StaticMethod)).First();
			InstanceMethodT = ClassT.Methods.Where(m => m.Name == nameof(Class.InstanceMethod)).First();
			VoidMethodT = ClassT.Methods.Where(m => m.Name == nameof(Class.VoidMethod)).First();
			VirtualMethodT = ClassT.Methods.Where(m => m.Name == nameof(Class.VirtualMethod)).First();
			ManyParametersMethodT = ClassT.Methods.Where(m => m.Name == nameof(Class.ManyParametersMethod)).First();
			ArrayMethodT = ClassT.Methods.Where(m => m.Name == nameof(Class.ArrayMethod)).First();
		}

		private static bool IsEmpty(CilMachine.RoutineContext ctx)
		{
			return ctx.Objects.Count == 0
				&& ctx.Vars.All(x => x is null)
				&& ctx.Stack.Count == 0
				&& ctx.Expressions.Count == 0;
		}

		private static ParameterDefinition Arg(int index)
		{
			var p = new ParameterDefinition(IntT);
			typeof(ParameterDefinition)
				.GetField("index", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField)
				.SetValue(p, index);
			return p;
		}

		private static VariableDefinition Var(int index)
		{
			var v = new VariableDefinition(IntT);
			typeof(VariableDefinition)
				.GetField("index", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.SetField)
				.SetValue(v, index);
			return v;
		}

		[TestMethod]
		public void Add()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Add);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Add(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Add_Ovf()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Add_Ovf);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Add(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Add_Ovf_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Add_Ovf_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Add(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void And()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.And);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.And(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Arglist()
		{
			var method = new MethodDefinition("test", 0, IntT);
			var ctx = new CilMachine.RoutineContext(method, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Arglist);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Arglist(PT, method));
		}

		[TestMethod]
		public void Beq()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Beq, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1)); // left
			ctx.Stack.Push(new Cil.Int32(PT, 2)); // right

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Beq(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Beq(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Beq_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Beq_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1)); // left
			ctx.Stack.Push(new Cil.Int32(PT, 2)); // right

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Beq(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Beq(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Bge()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Bge, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Bge(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Bge(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Bge_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Bge_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Bge(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Bge(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Bge_Un()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Bge_Un, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Bge(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Bge(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Bge_Un_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Bge_Un_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Bge(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Bge(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Bgt()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Bgt, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Bgt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Bgt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Bgt_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Bgt_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Bgt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Bgt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Bgt_Un()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Bgt_Un, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Bgt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Bgt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Bgt_Un_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Bgt_Un_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Bgt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Bgt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Ble()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Ble, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Ble(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Ble(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Ble_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Ble_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Ble(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Ble(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Ble_Un()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Ble_Un, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Ble(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Ble(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Ble_Un_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Ble_Un_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Ble(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Ble(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Blt()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Blt, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Blt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Blt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Blt_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Blt_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Blt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Blt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Blt_Un()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Blt_Un, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Blt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Blt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Blt_Un_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Blt_Un_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Blt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Blt(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Bne_Un()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Bne_Un, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Bne(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Bne(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Bne_Un_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Bne_Un_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Bne(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Bne(null, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)),
					0));
		}

		[TestMethod]
		public void Box()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			var instruction = Instruction.Create(OpCodes.Box, IntT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Box(PT, new Cil.Int32(PT, 42)));
		}

		[TestMethod]
		public void Br()
		{
			var nop = Instruction.Create(OpCodes.Nop); // next instruction
			var dup = Instruction.Create(OpCodes.Dup); // adjacent instruction
			var instruction = Instruction.Create(OpCodes.Br, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertEmpty();
			branches.AssertEmpty();
		}

		[TestMethod]
		public void Br_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // next instruction
			var dup = Instruction.Create(OpCodes.Dup); // adjacent instruction
			var instruction = Instruction.Create(OpCodes.Br_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertEmpty();
			branches.AssertEmpty();
		}

		[TestMethod]
		public void Break()
		{
			var ctx = new CilMachine.RoutineContext();
			var instruction = Instruction.Create(OpCodes.Break);

			CilMachine.Eval(ctx, instruction);

			Assert.IsTrue(IsEmpty(ctx));
		}

		[TestMethod]
		public void Brfalse()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Brfalse, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Brfalse(null, new Cil.Int32(PT, 42)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Brfalse(null, new Cil.Int32(PT, 42)),
					0));
		}

		[TestMethod]
		public void Brfalse_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Brfalse_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Brfalse(null, new Cil.Int32(PT, 42)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Brfalse(null, new Cil.Int32(PT, 42)),
					0));
		}

		[TestMethod]
		public void Brtrue()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Brtrue, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Brtrue(null, new Cil.Int32(PT, 42)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Brtrue(null, new Cil.Int32(PT, 42)),
					0));
		}

		[TestMethod]
		public void Brtrue_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // true branch
			var dup = Instruction.Create(OpCodes.Dup); // false branch
			var instruction = Instruction.Create(OpCodes.Brtrue_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Brtrue(null, new Cil.Int32(PT, 42)),
					1));

			var altBranch = branches[0];
			altBranch.StartInstruction.AssertEquals(dup);
			altBranch.Conditions.AssertContains(
				new Condition(
					new Cil.Brtrue(null, new Cil.Int32(PT, 42)),
					0));
		}

		[TestMethod]
		public void Call_StaticMethod()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.String(PT, "foo"));
			var instruction = Instruction.Create(OpCodes.Call, StaticMethodT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(
				new Cil.Call(PT, null, StaticMethodT, [new Cil.Int32(PT, 42), new Cil.String(PT, "foo")]));
		}

		[TestMethod]
		public void Call_InstanceMethod()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(new Cil.This(PT, ClassT));
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.String(PT, "foo"));
			var instruction = Instruction.Create(OpCodes.Call, InstanceMethodT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(
				new Cil.Call(PT, new Cil.This(PT, ClassT), InstanceMethodT, [new Cil.Int32(PT, 42), new Cil.String(PT, "foo")]));
		}

		[TestMethod]
		public void Call_VoidMethod()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(new Cil.This(PT, ClassT));
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.String(PT, "foo"));
			var instruction = Instruction.Create(OpCodes.Call, VoidMethodT);

			CilMachine.Eval(ctx, instruction);

			// do not push a result on stack if method has void result type
			ctx.Stack.AssertEmpty();
		}

		[TestMethod]
		public void Callvirt()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(new Cil.This(PT, ClassT));
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.String(PT, "foo"));
			var instruction = Instruction.Create(OpCodes.Callvirt, VirtualMethodT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(
				new Cil.Call(PT, new Cil.This(PT, ClassT), VirtualMethodT, [new Cil.Int32(PT, 42), new Cil.String(PT, "foo")]));
		}

		[TestMethod]
		public void Castclass()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			var instruction = Instruction.Create(OpCodes.Castclass, ClassT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Castclass(PT, ClassT, new Cil.Int32(PT, 42)));
		}

		[TestMethod]
		public void Ceq()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Ceq);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Ceq(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Cgt()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Cgt);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Cgt(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Cgt_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Cgt_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Cgt(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Ckfinite()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			var instruction = Instruction.Create(OpCodes.Ckfinite);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Clt()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Clt);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Clt(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Clt_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Clt_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Clt(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Constrained()
		{
			var ctx = new CilMachine.RoutineContext();
			var instruction = Instruction.Create(OpCodes.Constrained, ClassT);

			CilMachine.Eval(ctx, instruction);

			Assert.IsTrue(IsEmpty(ctx));
		}

		[TestMethod]
		public void Conv_I()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_I);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_I1()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_I1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I1(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_I2()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_I2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I2(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_I4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_I4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I4(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_I8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_I8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I8(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I1()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I1(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I1_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I1_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I1(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I2()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I2(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I2_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I2_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I2(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I4(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I4_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I4_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I4(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I8(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_I8_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_I8_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_I8(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_U()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_U);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_U_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_U_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_U1()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_U1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U1(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_U1_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_U1_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U1(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_U2()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_U2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U2(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_U2_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_U2_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U2(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_U4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_U4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U4(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_U4_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_U4_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U4(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_U8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_U8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U8(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_Ovf_U8_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_Ovf_U8_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U8(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_R_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_R_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_R4(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_R4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_R4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_R4(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_R8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_R8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_R8(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_U()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_U);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_U1()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_U1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U1(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_U2()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_U2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U2(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_U4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_U4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U4(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Conv_U8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Null.Instance);
			var instruction = Instruction.Create(OpCodes.Conv_U8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Conv_U8(PT, Cil.Null.Instance));
		}

		[TestMethod]
		public void Cpblk()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(new Cil.Int32(PT, 2000)); // destination address
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // source address
			ctx.Stack.Push(new Cil.Int32(PT, 10)); // number of bytes to copy
			var instruction = Instruction.Create(OpCodes.Cpblk);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
		}

		[TestMethod]
		public void Cpobj()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 2000)); // destination object address
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // source object address
			var instruction = Instruction.Create(OpCodes.Cpobj, ClassT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
		}

		[TestMethod]
		public void Div()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 10));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Div);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(
				new Cil.Div(PT, new Cil.Int32(PT, 10), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Div_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 10));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Div_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(
				new Cil.Div(PT, new Cil.Int32(PT, 10), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Dup()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			var instruction = Instruction.Create(OpCodes.Dup);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(2);
			ctx.Stack.Pop().AssertEquals(new Cil.Int32(PT, 42));
			ctx.Stack.Pop().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Endfilter()
		{
			var ctx = new CilMachine.RoutineContext();
			var instruction = Instruction.Create(OpCodes.Endfilter);

			CilMachine.Eval(ctx, instruction);

			Assert.IsTrue(IsEmpty(ctx));
		}

		[TestMethod]
		public void Endfinally()
		{
			var ctx = new CilMachine.RoutineContext();
			var instruction = Instruction.Create(OpCodes.Endfinally);

			CilMachine.Eval(ctx, instruction);

			Assert.IsTrue(IsEmpty(ctx));
		}

		[TestMethod]
		public void Exception()
		{
			var handler = Instruction.Create(OpCodes.Nop);
			handler.Offset = 42;
			var ctx = new CilMachine.RoutineContext(StaticMethodT, stackSize: 1, exceptionHandlers: [handler]);
			var instruction = Instruction.Create(OpCodes.Nop);
			instruction.Offset = 42;

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(Cil.Exception.Instance);
		}

		[TestMethod]
		public void Initblk()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // starting address
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // initialization value
			ctx.Stack.Push(new Cil.Int32(PT, 10)); // number of bytes to initialize
			var instruction = Instruction.Create(OpCodes.Initblk);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
		}

		[TestMethod]
		public void Initobj()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // address
			var instruction = Instruction.Create(OpCodes.Initobj, ClassT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
		}

		[TestMethod]
		public void Isinst()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.String(PT, "foo"));
			var instruction = Instruction.Create(OpCodes.Isinst, StringT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(
				new Cil.Isinst(PT, StringT, new Cil.String(PT, "foo")));
		}

		[TestMethod]
		public void Jmp()
		{
			var ctx = new CilMachine.RoutineContext();
			var instruction = Instruction.Create(OpCodes.Jmp, VoidMethodT);

			CilMachine.Eval(ctx, instruction);

			Assert.IsTrue(IsEmpty(ctx));
		}

		[TestMethod]
		public void Ldarg_HasValue()
		{
			var ctx = new CilMachine.RoutineContext(StaticMethodT, stackSize: 1);
			ctx.Args[1] = new Cil.Int32(PT, 42);
			var instruction = Instruction.Create(OpCodes.Ldarg, Arg(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldarg_InstanceMethod()
		{
			var ctx = new CilMachine.RoutineContext(InstanceMethodT, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldarg, Arg(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Arg(PT, 0, InstanceMethodT.Parameters[0]));
		}

		[TestMethod]
		public void Ldarg_StaticMethod()
		{
			var ctx = new CilMachine.RoutineContext(StaticMethodT, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldarg, Arg(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Arg(PT, 1, StaticMethodT.Parameters[1]));
		}

		[TestMethod]
		public void Ldarg_This()
		{
			var ctx = new CilMachine.RoutineContext(InstanceMethodT, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldarg, Arg(0));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.This(PT, ClassT));
		}

		[TestMethod]
		public void Ldarg_0()
		{
			var ctx = new CilMachine.RoutineContext(ManyParametersMethodT, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldarg_0);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Arg(PT, 0, ManyParametersMethodT.Parameters[0]));
		}

		[TestMethod]
		public void Ldarg_1()
		{
			var ctx = new CilMachine.RoutineContext(ManyParametersMethodT, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldarg_1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Arg(PT, 1, ManyParametersMethodT.Parameters[1]));
		}

		[TestMethod]
		public void Ldarg_2()
		{
			var ctx = new CilMachine.RoutineContext(ManyParametersMethodT, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldarg_2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Arg(PT, 2, ManyParametersMethodT.Parameters[2]));
		}

		[TestMethod]
		public void Ldarg_3()
		{
			var ctx = new CilMachine.RoutineContext(ManyParametersMethodT, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldarg_3);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Arg(PT, 3, ManyParametersMethodT.Parameters[3]));
		}

		[TestMethod]
		public void Ldarg_S()
		{
			var ctx = new CilMachine.RoutineContext(ManyParametersMethodT, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldarg_S, Arg(4));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Arg(PT, 4, ManyParametersMethodT.Parameters[4]));
		}

		[TestMethod]
		public void Ldarga()
		{
			var ctx = new CilMachine.RoutineContext(ManyParametersMethodT, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldarga, Arg(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Argptr(PT, 1));
		}

		[TestMethod]
		public void Ldarga_S()
		{
			var ctx = new CilMachine.RoutineContext(ManyParametersMethodT, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldarga_S, Arg(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Argptr(PT, 1));
		}

		[TestMethod]
		public void Ldc_I4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4, 42);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldc_I4_0()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4_0);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 0));
		}

		[TestMethod]
		public void Ldc_I4_1()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4_1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 1));
		}

		[TestMethod]
		public void Ldc_I4_2()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4_2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 2));
		}

		[TestMethod]
		public void Ldc_I4_3()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4_3);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 3));
		}

		[TestMethod]
		public void Ldc_I4_4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4_4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 4));
		}

		[TestMethod]
		public void Ldc_I4_5()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4_5);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 5));
		}

		[TestMethod]
		public void Ldc_I4_6()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4_6);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 6));
		}

		[TestMethod]
		public void Ldc_I4_7()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4_7);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 7));
		}

		[TestMethod]
		public void Ldc_I4_8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4_8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 8));
		}

		[TestMethod]
		public void Ldc_I4_M1()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4_M1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, -1));
		}

		[TestMethod]
		public void Ldc_I4_S()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)42);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldc_I8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_I8, (long)42);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int64(PT, 42));
		}

		[TestMethod]
		public void Ldc_R4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_R4, 3.14f);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Float32(PT, 3.14f));
		}

		[TestMethod]
		public void Ldc_R8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldc_R8, (double)3.14f);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Float64(PT, 3.14f));
		}

		[TestMethod]
		public void Ldelem_HasValue()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_Any, IntT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelem_NoValue()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Call(PT, null, ArrayMethodT, []));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_Any, IntT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(
				new Cil.Elem(PT,
					new Cil.Call(PT, null, ArrayMethodT, []),
					new Cil.Int32(PT, 1)));
		}

		[TestMethod]
		public void Ldelem_I()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_I);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelem_I1()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_I1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelem_I2()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_I2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelem_I4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_I4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelem_I8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_I8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelem_R4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_R4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelem_R8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_R8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelem_Ref()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_Ref);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelem_U1()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_U1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelem_U2()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_U2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelem_U4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Arrays.Add(new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1)), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelem_U4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldelema()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			var array = new Cil.Array(PT, IntT, new Cil.Int32(PT, 5));
			ctx.Stack.Push(array);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Ldelema, IntT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(
				new Cil.Elemptr(PT,
					new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)),
					new Cil.Int32(PT, 1)));
		}

		[TestMethod]
		public void Ldfld()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.This(PT, ClassT));
			var instruction = Instruction.Create(OpCodes.Ldfld, IntFieldT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Fld(PT, new Cil.This(PT, ClassT), IntFieldT));
		}

		[TestMethod]
		public void Ldflda()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.This(PT, ClassT));
			var instruction = Instruction.Create(OpCodes.Ldflda, IntFieldT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Fld(PT, new Cil.This(PT, ClassT), IntFieldT));
		}

		[TestMethod]
		public void Ldftn()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldftn, InstanceMethodT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Ftn(PT, null, InstanceMethodT));
		}

		[TestMethod]
		public void Ldind_I_HasValue()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_I);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldind_I_NoValue()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_I);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Heapptr(PT, new Cil.Int32(PT, 1000)));
		}

		[TestMethod]
		public void Ldind_I1()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_I1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldind_I2()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_I2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldind_I4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_I4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldind_I8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_I8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldind_R4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_R4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldind_R8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_R8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldind_Ref()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.This(PT, ClassT));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_Ref);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.This(PT, ClassT));
		}

		[TestMethod]
		public void Ldind_U1()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_U1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldind_U2()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_U2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldind_U4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldind_U4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldlen()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
			var instruction = Instruction.Create(OpCodes.Ldlen);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Len(PT, new Cil.Array(PT, IntT, new Cil.Int32(PT, 5))));
		}

		[TestMethod]
		public void Ldloc()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 2, stackSize: 1);
			ctx.Vars[1] = new Cil.Int32(PT, 42);
			var instruction = Instruction.Create(OpCodes.Ldloc, Var(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldloc_S()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 2, stackSize: 1);
			ctx.Vars[1] = new Cil.Int32(PT, 42);
			var instruction = Instruction.Create(OpCodes.Ldloc_S, Var(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldloc_0()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 1, stackSize: 1);
			ctx.Vars[0] = new Cil.Int32(PT, 42);
			var instruction = Instruction.Create(OpCodes.Ldloc_0);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldloc_1()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 2, stackSize: 1);
			ctx.Vars[1] = new Cil.Int32(PT, 42);
			var instruction = Instruction.Create(OpCodes.Ldloc_1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldloc_2()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 3, stackSize: 1);
			ctx.Vars[2] = new Cil.Int32(PT, 42);
			var instruction = Instruction.Create(OpCodes.Ldloc_2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldloc_3()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 4, stackSize: 1);
			ctx.Vars[3] = new Cil.Int32(PT, 42);
			var instruction = Instruction.Create(OpCodes.Ldloc_3);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldloca_HasValue()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 2, stackSize: 1);
			ctx.Vars[1] = new Cil.Int32(PT, 42);
			var instruction = Instruction.Create(OpCodes.Ldloca, Var(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Ldloca_NoValue()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 2, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldloca, Var(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Varptr(PT, 1));
		}

		[TestMethod]
		public void Ldloca_S()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 2, stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldloca_S, Var(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Varptr(PT, 1));
		}

		[TestMethod]
		public void Ldnull()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldnull);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(Cil.Null.Instance);
		}

		[TestMethod]
		public void Ldobj()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Heap.Add(new Cil.Int32(PT, 1000), new Cil.This(PT, ClassT));
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Ldobj, ClassT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.This(PT, ClassT));
		}

		[TestMethod]
		public void Ldsfld()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldsfld, StringFieldT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Fld(PT, null, StringFieldT));
		}

		[TestMethod]
		public void Ldsflda()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldsflda, StringFieldT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Fld(PT, null, StringFieldT));
		}

		[TestMethod]
		public void Ldstr()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldstr, "foo");

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.String(PT, "foo"));
		}

		[TestMethod]
		public void Ldtoken_Typeof()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldtoken, IntT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Typeof(PT, IntT));
		}

		[TestMethod]
		public void Ldtoken_Methodof()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldtoken, InstanceMethodT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Methodof(PT, InstanceMethodT));
		}

		[TestMethod]
		public void Ldtoken_Fieldof()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Ldtoken, IntFieldT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Fieldof(PT, IntFieldT));
		}

		[TestMethod]
		public void Ldvirtftn()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.This(PT, ClassT));
			var instruction = Instruction.Create(OpCodes.Ldvirtftn, VirtualMethodT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Ftn(PT, new Cil.This(PT, ClassT), VirtualMethodT));
		}

		[TestMethod]
		public void Leave()
		{
			var nop = Instruction.Create(OpCodes.Nop); // next instruction
			var dup = Instruction.Create(OpCodes.Dup); // adjacent instruction
			var instruction = Instruction.Create(OpCodes.Leave, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertEmpty();
			branches.AssertEmpty();
		}

		[TestMethod]
		public void Leave_S()
		{
			var nop = Instruction.Create(OpCodes.Nop); // next instruction
			var dup = Instruction.Create(OpCodes.Dup); // adjacent instruction
			var instruction = Instruction.Create(OpCodes.Leave_S, nop);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertEmpty();
			branches.AssertEmpty();
		}

		[TestMethod]
		public void Localloc()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 10));
			var instruction = Instruction.Create(OpCodes.Localloc);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Bytes(PT, new Cil.Int32(PT, 10)));
		}

		[TestMethod]
		public void Mkrefany()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Mkrefany, ClassT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Mkref(PT, new Cil.Int32(PT, 1000), ClassT));
		}

		[TestMethod]
		public void Mul()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Mul);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Mul(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Mul_Ovf()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Mul_Ovf);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Mul(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Mul_Ovf_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Mul_Ovf_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Mul(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Neg()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Neg);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Neg(PT, new Cil.Int32(PT, 1)));
		}

		[TestMethod]
		public void Newarr()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 5));
			var instruction = Instruction.Create(OpCodes.Newarr, IntT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)));
		}

		[TestMethod]
		public void Newobj()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			ctx.Stack.Push(new Cil.String(PT, "foo"));
			var instruction = Instruction.Create(OpCodes.Newobj, ConstructorT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(
				new Cil.Newobj(PT,
					ClassT,
					ConstructorT,
					[new Cil.Int32(PT, 42), new Cil.String(PT, "foo")]));
		}

		[TestMethod]
		public void Nop()
		{
			var ctx = new CilMachine.RoutineContext();
			var instruction = Instruction.Create(OpCodes.Nop);

			CilMachine.Eval(ctx, instruction);

			Assert.IsTrue(IsEmpty(ctx));
		}

		[TestMethod]
		public void Not()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			var instruction = Instruction.Create(OpCodes.Not);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Not(PT, new Cil.Int32(PT, 42)));
		}

		[TestMethod]
		public void Or()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Or);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Or(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Pop()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			var instruction = Instruction.Create(OpCodes.Pop);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
		}

		[TestMethod]
		public void Readonly()
		{
			var ctx = new CilMachine.RoutineContext();
			var instruction = Instruction.Create(OpCodes.Readonly);

			CilMachine.Eval(ctx, instruction);

			Assert.IsTrue(IsEmpty(ctx));
		}

		[TestMethod]
		public void Refanytype()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Refanytype);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Refanytype(PT, new Cil.Int32(PT, 1000)));
		}

		[TestMethod]
		public void Refanyval()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 1000));
			var instruction = Instruction.Create(OpCodes.Refanyval, ClassT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Refanyval(PT, ClassT, new Cil.Int32(PT, 1000)));
		}

		[TestMethod]
		public void Rem()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 10));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Rem);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Rem(PT, new Cil.Int32(PT, 10), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Rem_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 10));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Rem_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Rem(PT, new Cil.Int32(PT, 10), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Ret()
		{
			var dup = Instruction.Create(OpCodes.Dup); // adjacent instruction
			var instruction = Instruction.Create(OpCodes.Ret);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertNull();
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertEmpty();
			branches.AssertEmpty();
		}

		[TestMethod]
		public void Rethrow()
		{
			var dup = Instruction.Create(OpCodes.Dup); // adjacent instruction
			var instruction = Instruction.Create(OpCodes.Rethrow);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 2);

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertNull();
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertEmpty();
			branches.AssertEmpty();
		}

		[TestMethod]
		public void Shl()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 10));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Shl);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Shl(PT, new Cil.Int32(PT, 10), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Shr()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 10));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Shr);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Shr(PT, new Cil.Int32(PT, 10), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Shr_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 10));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Shr_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Shr(PT, new Cil.Int32(PT, 10), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Sizeof()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Sizeof, ClassT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Sizeof(PT, ClassT));
		}

		[TestMethod]
		public void Starg()
		{
			var ctx = new CilMachine.RoutineContext(InstanceMethodT, stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			var instruction = Instruction.Create(OpCodes.Starg, Arg(0));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Args[0].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Starg_S()
		{
			var ctx = new CilMachine.RoutineContext(StaticMethodT, stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			var instruction = Instruction.Create(OpCodes.Starg_S, Arg(0));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Args[0].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stelem()
		{
			var array = new Cil.Array(PT, IntT, new Cil.Int32(PT, 5));
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(array); // array
			ctx.Stack.Push(new Cil.Int32(PT, 1)); // index
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stelem_Any, IntT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Arrays[new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1))].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stelem_I()
		{
			var array = new Cil.Array(PT, IntT, new Cil.Int32(PT, 5));
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(array); // array
			ctx.Stack.Push(new Cil.Int32(PT, 1)); // index
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stelem_I);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Arrays[new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1))].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stelem_I1()
		{
			var array = new Cil.Array(PT, IntT, new Cil.Int32(PT, 5));
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(array); // array
			ctx.Stack.Push(new Cil.Int32(PT, 1)); // index
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stelem_I1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Arrays[new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1))].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stelem_I2()
		{
			var array = new Cil.Array(PT, IntT, new Cil.Int32(PT, 5));
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(array); // array
			ctx.Stack.Push(new Cil.Int32(PT, 1)); // index
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stelem_I2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Arrays[new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1))].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stelem_I4()
		{
			var array = new Cil.Array(PT, IntT, new Cil.Int32(PT, 5));
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(array); // array
			ctx.Stack.Push(new Cil.Int32(PT, 1)); // index
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stelem_I4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Arrays[new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1))].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stelem_I8()
		{
			var array = new Cil.Array(PT, IntT, new Cil.Int32(PT, 5));
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(array); // array
			ctx.Stack.Push(new Cil.Int32(PT, 1)); // index
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stelem_I8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Arrays[new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1))].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stelem_R4()
		{
			var array = new Cil.Array(PT, IntT, new Cil.Int32(PT, 5));
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(array); // array
			ctx.Stack.Push(new Cil.Int32(PT, 1)); // index
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stelem_R4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Arrays[new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1))].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stelem_R8()
		{
			var array = new Cil.Array(PT, IntT, new Cil.Int32(PT, 5));
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(array); // array
			ctx.Stack.Push(new Cil.Int32(PT, 1)); // index
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stelem_R8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Arrays[new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1))].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stelem_Ref()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 3);
			ctx.Stack.Push(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5))); // array
			ctx.Stack.Push(new Cil.Int32(PT, 1)); // index
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stelem_Ref);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Arrays.AssertCount(1);
			ctx.Arrays[new CilMachine.ArrayIndex(new Cil.Array(PT, IntT, new Cil.Int32(PT, 5)), new Cil.Int32(PT, 1))].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stfld()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.This(PT, ClassT)); // instance
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stfld, IntFieldT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Objects[new CilMachine.ObjectField(new Cil.This(PT, ClassT), IntFieldT)].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stind_I()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // address
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stind_I);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Heap[new Cil.Int32(PT, 1000)].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stind_I1()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // address
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stind_I1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Heap[new Cil.Int32(PT, 1000)].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stind_I2()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // address
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stind_I2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Heap[new Cil.Int32(PT, 1000)].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stind_I4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // address
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stind_I4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Heap[new Cil.Int32(PT, 1000)].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stind_I8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // address
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stind_I8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Heap[new Cil.Int32(PT, 1000)].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stind_R4()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // address
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stind_R4);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Heap[new Cil.Int32(PT, 1000)].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stind_R8()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // address
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stind_R8);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Heap[new Cil.Int32(PT, 1000)].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stind_Ref()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // address
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stind_Ref);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Heap[new Cil.Int32(PT, 1000)].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stloc()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 2, stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stloc, Var(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Vars[1].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stloc_0()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 1, stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stloc_0);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Vars[0].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stloc_1()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 2, stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stloc_1);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Vars[1].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stloc_2()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 3, stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stloc_2);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Vars[2].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stloc_3()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 4, stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stloc_3);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Vars[3].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stloc_S()
		{
			var ctx = new CilMachine.RoutineContext(varCount: 2, stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42)); // value
			var instruction = Instruction.Create(OpCodes.Stloc_S, Var(1));

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Vars[1].AssertEquals(new Cil.Int32(PT, 42));
		}

		[TestMethod]
		public void Stobj()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1000)); // address
			ctx.Stack.Push(new Cil.This(PT, ClassT)); // value
			var instruction = Instruction.Create(OpCodes.Stobj, ClassT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Heap[new Cil.Int32(PT, 1000)].AssertEquals(new Cil.This(PT, ClassT));
		}

		[TestMethod]
		public void Stsfld()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.String(PT, "foo")); // value
			var instruction = Instruction.Create(OpCodes.Stsfld, StringFieldT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertEmpty();
			ctx.Objects[new CilMachine.ObjectField(null, StringFieldT)].AssertEquals(new Cil.String(PT, "foo"));
		}

		[TestMethod]
		public void Sub()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Sub);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Sub(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Sub_Ovf()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Sub_Ovf);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Sub(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Sub_Ovf_Un()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Sub_Ovf_Un);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Sub(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}

		[TestMethod]
		public void Switch()
		{
			var nop = Instruction.Create(OpCodes.Nop); // 0 branch
			var dup = Instruction.Create(OpCodes.Dup); // 1 branch
			var ret = Instruction.Create(OpCodes.Ret); // 2 instruction
			var pop = Instruction.Create(OpCodes.Pop); // adjacent instruction
			var instruction = Instruction.Create(OpCodes.Switch, [nop, dup, ret]);
			instruction.Next = pop;

			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 1));

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			branches.AssertCount(2);

			// 0 branch
			nextInstruction.AssertEquals(nop);
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertContains(
				new Condition(
					new Cil.Switch(null, new Cil.Int32(PT, 1)),
					0));

			// 1 branch
			var branch1 = branches[0];
			branch1.StartInstruction.AssertEquals(dup);
			branch1.Conditions.AssertContains(
				new Condition(
					new Cil.Switch(null, new Cil.Int32(PT, 1)),
					1));

			// 2 branch
			var branch2 = branches[1];
			branch2.StartInstruction.AssertEquals(ret);
			branch2.Conditions.AssertContains(
				new Condition(
					new Cil.Switch(null, new Cil.Int32(PT, 1)),
					2));
		}

		[TestMethod]
		public void Tail()
		{
			var ctx = new CilMachine.RoutineContext();
			var instruction = Instruction.Create(OpCodes.Tail);

			CilMachine.Eval(ctx, instruction);

			Assert.IsTrue(IsEmpty(ctx));
		}

		[TestMethod]
		public void Throw()
		{
			var dup = Instruction.Create(OpCodes.Dup); // adjacent instruction
			var instruction = Instruction.Create(OpCodes.Throw);
			instruction.Next = dup;

			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(Cil.Exception.Instance);

			var branches = new List<CilMachine.RoutineContext>();
			CilMachine.Eval(ctx, branches, instruction, out var nextInstruction);

			nextInstruction.AssertNull();
			ctx.Stack.AssertEmpty();
			ctx.Conditions.AssertEmpty();
			branches.AssertEmpty();
		}

		[TestMethod]
		public void Unaligned()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			var instruction = Instruction.Create(OpCodes.Unaligned, (byte)128);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Unaligned(PT, 128));
		}

		[TestMethod]
		public void Unbox()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.Int32(PT, 42));
			var instruction = Instruction.Create(OpCodes.Unbox, IntT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Unbox(PT, IntT, new Cil.Int32(PT, 42)));
		}

		[TestMethod]
		public void Unbox_Any()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 1);
			ctx.Stack.Push(new Cil.This(PT, ClassT));
			var instruction = Instruction.Create(OpCodes.Unbox_Any, ClassT);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Unbox(PT, ClassT, new Cil.This(PT, ClassT)));
		}

		[TestMethod]
		public void Volatile()
		{
			var ctx = new CilMachine.RoutineContext();
			var instruction = Instruction.Create(OpCodes.Volatile);

			CilMachine.Eval(ctx, instruction);

			Assert.IsTrue(IsEmpty(ctx));
		}

		[TestMethod]
		public void Xor()
		{
			var ctx = new CilMachine.RoutineContext(stackSize: 2);
			ctx.Stack.Push(new Cil.Int32(PT, 1));
			ctx.Stack.Push(new Cil.Int32(PT, 2));
			var instruction = Instruction.Create(OpCodes.Xor);

			CilMachine.Eval(ctx, instruction);

			ctx.Stack.AssertCount(1);
			ctx.Stack.Peek().AssertEquals(new Cil.Xor(PT, new Cil.Int32(PT, 1), new Cil.Int32(PT, 2)));
		}
	}
}
#pragma warning restore CS0618
