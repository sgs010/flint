using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class String : Ast
	{
		public readonly string Value;
		public String(SequencePoint sp, string value) : base(sp)
		{
			Value = value;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(String), Value);
		}

		public override bool Equals(Ast other)
		{
			if (other is String str)
			{
				return Value.Equals(str.Value);
			}
			return false;
		}
	}
}
