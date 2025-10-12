using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Arg : Ast
	{
		public readonly int Number;
		public readonly ParameterDefinition Parameter;
		public Arg(CilPoint pt, int number, ParameterDefinition parameter) : base(pt)
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
