namespace Flint.Vm
{
	abstract class Ast : IEquatable<Ast>
	{
		protected abstract IEnumerable<Ast> GetChildren();
		public abstract bool Equals(Ast other);
	}
}
