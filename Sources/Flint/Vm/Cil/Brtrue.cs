using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Brtrue : Ast
	{
		public readonly Ast Value;
		public Brtrue(SequencePoint sp, Ast val) : base(sp)
		{
			Value = val;
		}

		public static Brtrue Create(SequencePoint sp, Ast val)
		{
			return new Brtrue(sp, val);
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Brtrue), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Brtrue br)
			{
				return Value.Equals(br.Value);
			}
			return false;
		}
	}
}
