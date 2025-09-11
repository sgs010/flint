using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class String : Ast
	{
		public readonly string Value;
		public String(SequencePoint debug, string value) : base(debug)
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
