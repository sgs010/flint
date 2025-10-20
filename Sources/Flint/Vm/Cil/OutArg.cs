using Flint.Common;

namespace Flint.Vm.Cil
{
	class OutArg : Ast
	{
		public readonly Call Call;
		public readonly int Index;
		public OutArg(CilPoint pt, Call call, int index) : base(pt)
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
				return Are.Equal(Call, arg.Call)
					&& Index == arg.Index;
			}
			return false;
		}
	}
}
