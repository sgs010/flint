using Flint.Common;
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
			return HashCode.Combine(typeof(CilPoint), Hash.Code(Method), Offset);
		}

		public override bool Equals(object obj)
		{
			if (obj is CilPoint pt)
			{
				return Are.Equal(Method, pt.Method)
					&& Offset == pt.Offset;
			}
			return false;
		}
	}
}
