namespace Flint.Vm.Cil
{
	abstract class Literal<TValue, TImpl> : Ast
		where TImpl : Literal<TValue, TImpl>
	{
		public readonly TValue Value;
		protected Literal(CilPoint pt, TValue value) : base(pt)
		{
			Value = value;
		}

		public sealed override IEnumerable<Ast> GetChildren()
		{
			yield break;
		}

		public sealed override int GetHashCode()
		{
			return HashCode.Combine(typeof(TImpl), Value);
		}

		public sealed override bool Equals(Ast other)
		{
			if (other is TImpl impl)
			{
				return Value.Equals(impl.Value);
			}
			return false;
		}

		protected sealed override (Ast, MergeResult) Merge(Ast other)
		{
			return OkMerged(other);
		}
	}
}
