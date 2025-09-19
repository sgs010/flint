using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Arg : Ast
	{
		public readonly int Number;
		public readonly ParameterReference Reference;
		public Arg(SequencePoint sp, int number, ParameterReference reference) : base(sp)
		{
			Number = number;
			Reference = reference;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Arg), Number, Reference);
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
