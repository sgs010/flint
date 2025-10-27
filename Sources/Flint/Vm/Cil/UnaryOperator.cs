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

		protected sealed override (Ast, MergeResult) Merge(Ast other)
		{
			if (other is T t)
			{
				var (value, valueMr) = Merge(Value, t.Value);
				if (valueMr == MergeResult.NotMerged)
					return NotMerged();

				return OkMerged(CreateInstance(CilPoint, value));
			}
			return NotMerged();
		}
	}
}
