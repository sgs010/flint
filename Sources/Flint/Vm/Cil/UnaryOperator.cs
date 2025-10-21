using Flint.Common;

namespace Flint.Vm.Cil
{
	abstract class UnaryOperator<T> : Ast
		where T : UnaryOperator<T>
	{
		public readonly Ast Value;
		protected UnaryOperator(CilPoint pt, Ast value) : base(pt)
		{
			Value = value;
		}

		protected abstract T CreateInstance(CilPoint pt, Ast value);

		public sealed override IEnumerable<Ast> GetChildren()
		{
			yield return Value;
		}

		public sealed override int GetHashCode()
		{
			return HashCode.Combine(typeof(T), Value);
		}

		public sealed override bool Equals(Ast other)
		{
			if (other is T t)
			{
				return Are.Equal(Value, t.Value);
			}
			return false;
		}

		protected sealed override (Ast, bool) Merge(Ast other)
		{
			if (other is T t)
			{
				var (value, valueOk) = Merge(Value, t.Value);
				if (valueOk == false)
					return (null, false);

				var merged = CreateInstance(CilPoint, value);
				return (merged, true);
			}
			return (null, false);
		}
	}
}
