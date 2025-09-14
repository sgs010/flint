using Mono.Cecil.Cil;

namespace Flint.Vm.Cil
{
	class OutArg : Ast
	{
		public readonly Call Call;
		public readonly int Index;
		public OutArg(SequencePoint sp, Call call, int index) : base(sp)
		{
			Call = call;
			Index = index;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Call;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(OutArg), Call, Index);
		}

		public override bool Equals(Ast other)
		{
			if (other is OutArg arg)
			{
				return Call.Equals(arg.Call)
					&& Index.Equals(arg.Index);
			}
			return false;
		}
	}
}
