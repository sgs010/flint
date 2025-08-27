using Flint.Vm;
using FluentAssertions;
using Mono.Cecil.Cil;

namespace FlintTests
{
	[TestClass]
	public class EvalMachineTests
	{
		private static bool IsEmpty(EvalMachine.RoutineContext ctx)
		{
			return ctx.Memory.Count == 0
				&& ctx.Variables.All(x => x is null)
				&& ctx.Stack.Count == 0
				&& ctx.Expressions.Count == 0;
		}

		[TestMethod]
		public void Nop()
		{
			var ctx = new EvalMachine.RoutineContext(1, 2);
			var instruction = Instruction.Create(OpCodes.Nop);

			EvalMachine.Eval(ctx, instruction);

			IsEmpty(ctx).Should().BeTrue();
		}
	}
}
