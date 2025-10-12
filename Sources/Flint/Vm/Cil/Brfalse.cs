namespace Flint.Vm.Cil
{
	class Brfalse : Ast
	{
		public readonly Ast Value;
		public Brfalse(CilPoint pt, Ast val) : base(pt)
		{
			Value = val;
		}

		public static Brfalse Create(CilPoint pt, Ast val)
		{
			return new Brfalse(pt, val);
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
