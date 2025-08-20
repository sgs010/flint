using Mono.Cecil;

namespace Flint.Vm.Cil
{
	class Arg : Ast
	{
		public readonly int Number;
		public readonly ParameterReference Reference;
		public Arg(int number, ParameterReference reference)
		{
			Number = number;
			Reference = reference;
		}

		protected override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Number, Reference);
		}

		public override bool Equals(Ast other)
		{
			if (other is Arg arg)
			{
				return Number.Equals(arg.Number)
					&& Reference.Equals(arg.Reference);
			}
			return false;
		}
	}
}
