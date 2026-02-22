using Flint.Common;
using Flint.Vm.Cil;

namespace Flint.Vm
{
	sealed class AstEqualityComparer
		: IEqualityComparer<Ast>
		, IEqualityComparer<Cil.Call>
	{
		public static readonly AstEqualityComparer Instance = new();

		int IEqualityComparer<Ast>.GetHashCode(Ast obj)
		{
			return HashCode.Combine(obj);
		}

		bool IEqualityComparer<Ast>.Equals(Ast x, Ast y)
		{
			return Are.Equal(x, y);
		}

		int IEqualityComparer<Call>.GetHashCode(Cil.Call obj)
		{
			return HashCode.Combine(obj);
		}

		bool IEqualityComparer<Call>.Equals(Cil.Call x, Cil.Call y)
		{
			return Are.Equal(x, y);
		}
	}
}
