using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Arg : Ast
	{
		public readonly int Number;
		public readonly ParameterDefinition Parameter;
		public Arg(SequencePoint sp, int number, ParameterDefinition parameter) : base(sp)
		{
			Number = number;
			Parameter = parameter;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Arg), Number, Parameter);
		}

		public override bool Equals(Ast other)
		{
			if (other is Arg arg)
			{
				return Number.Equals(arg.Number)
					&& Parameter.Equals(arg.Parameter);
			}
			return false;
		}
	}
}
