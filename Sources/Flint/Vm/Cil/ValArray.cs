using Flint.Common;

namespace Flint.Vm.Cil
{
	class ValArray : Ast
	{
		public record struct Val(Ast Index, Ast Value);

		public readonly Array Array;
		public readonly Val[] Values;
		public ValArray(CilPoint pt, Array array, Val[] values) : base(pt)
		{
			Array = array;
			Values = values;
		}

		public override IEnumerable<Ast> GetChildren()
		{
			yield return Array;
			foreach (var x in Values)
			{
				yield return x.Index;
				yield return x.Value;
			}
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(typeof(ValArray), Array, Values);
		}

		public override bool Equals(Ast other)
		{
			if (other is ValArray va)
			{
				return Are.Equal(Array, va.Array)
					&& Are.Equal(Values, va.Values, Cmp);
			}
			return false;
		}

		private static bool Cmp(Val x, Val y)
		{
			return Are.Equal(x.Index, y.Index)
				&& Are.Equal(x.Value, y.Value);
		}

		protected sealed override Ast RewriteChildren(Func<Ast, (Ast, bool)> fn)
		{
			return this;
		}
	}
}
