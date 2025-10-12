namespace Flint.Vm.Cil
{
	class Brtrue : Ast
	{
		public readonly Ast Value;
		public Brtrue(CilPoint pt, Ast val) : base(pt)
		{
			Value = val;
		}

		public static Brtrue Create(CilPoint pt, Ast val)
		{
			return new Brtrue(pt, val);
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
