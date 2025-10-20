using Flint.Common;

namespace Flint.Vm.Cil
{
	class Bytes : Ast
	{
		public readonly Ast Count;
		public Bytes(CilPoint pt, Ast count) : base(pt)
		{
			Count = count;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Count;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(Bytes), Count);
		}

		public override bool Equals(Ast other)
		{
			if (other is Bytes bytes)
			{
				return Are.Equal(Count, bytes.Count);
			}
			return false;
		}
	}
}
