using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class Brfalse : Ast
	{
		public readonly Ast Value;
		public Brfalse(SequencePoint sp, Ast val) : base(sp)
		{
			Value = val;
		}

		public static Brfalse Create(SequencePoint sp, Ast val)
		{
			return new Brfalse(sp, val);
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Brfalse), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is Brfalse br)
			{
				return Value.Equals(br.Value);
			}
			return false;
		}
	}
}
