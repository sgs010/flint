using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Flint.Vm
{
	class CilPoint
	{
		public readonly MethodReference Method;
		public int Offset;
		public SequencePoint SequencePoint;
		public CilPoint(MethodReference method, int offset, SequencePoint sp)
		{
			Method = method;
			Offset = offset;
			SequencePoint = sp;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(CilPoint), Method.MetadataToken, Offset);
		}

		public override bool Equals(object obj)
		{
			if (obj is CilPoint pt)
			{
				return Method.MetadataToken.Equals(pt.Method.MetadataToken)
					&& Offset.Equals(pt.Offset);
			}
			return false;
		}
	}
}
