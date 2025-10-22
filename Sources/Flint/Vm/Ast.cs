namespace Flint.Vm
{
	abstract class Ast : IEquatable<Ast>
	{
		public readonly CilPoint CilPoint;
		protected Ast(CilPoint pt)
		{
			CilPoint = pt;
		}

		public abstract IEnumerable<Ast> GetChildren();
		public abstract bool Equals(Ast other);
		public virtual void Capture(Ast other, IDictionary<string, Ast> captures) { }
		protected virtual (Ast, bool) Merge(Ast other) { return (null, false); }

		public static (Ast, bool) Merge(Ast x, Ast y)
		{
			if (x == null && y == null)
				return (null, true);
			if (x != null && y == null)
				return (x, true);
			if (x == null && y != null)
				return (y, true);

			if (x.CilPoint.Equals(y.CilPoint) == false)
				return (null, false);

			if (x.Equals(y))
				return (x, true);

			var (merged, ok) = x.Merge(y);
			if (ok)
				return (merged, true);

			(merged, ok) = y.Merge(x);
			if (ok)
				return (merged, true);

			return (null, false);
		}

		public static (Ast[], bool) Merge(Ast[] x, Ast[] y)
		{
			if (x == null && y == null)
				return (null, true);
			if (x != null && y == null)
				return (x, true);
			if (x == null && y != null)
				return (y, true);

			if (x.Length == 0 && y.Length == 0)
				return (x, true);

			if (x.Length != y.Length)
				return (null, false);

			var merged = new Ast[x.Length];
			for (var i = 0; i < x.Length; ++i)
			{
				var (m, ok) = Merge(x[i], y[i]);
				if (ok == false)
					return (null, false);
				merged[i] = m;
			}
			return (merged, true);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is Ast ast ? Equals(ast) : false;
		}

		public (Dictionary<string, Ast> captures, bool ok) Match(Ast pattern, bool recursive = false)
		{
			var captures = new Dictionary<string, Ast>();
			var ok = Match(this, recursive, captures, pattern);
			return (captures, ok);
		}

		private static bool Match(Ast root, bool recursive, IDictionary<string, Ast> captures, Ast pattern)
		{
			var anyMatch = false;
			if (pattern.Equals(root))
			{
				anyMatch = true;
				Capture(root, captures, pattern);
			}
			if (recursive)
			{
				foreach (var child in root.GetChildren())
				{
					if (child != null)
						anyMatch |= Match(child, true, captures, pattern);
				}
			}
			return anyMatch;
		}

		private static void Capture(Ast root, IDictionary<string, Ast> captures, Ast pattern)
		{
			var patternNodes = BFS(pattern, -1);
			var rootNodes = BFS(root, patternNodes.Count);
			for (var i = 0; i < patternNodes.Count; ++i)
			{
				var p = patternNodes[i];
				var r = rootNodes[i];
				if (p != null)
					p.Capture(r, captures);
			}
		}

		private static List<Ast> BFS(Ast root, int maxCount)
		{
			var nodes = new List<Ast>(maxCount > 0 ? maxCount : 0);
			BFS(root, nodes, maxCount);
			return nodes;
		}

		private static void BFS(Ast root, List<Ast> nodes, int maxCount)
		{
			if (maxCount >= 0 && nodes.Count >= maxCount)
				return;

			nodes.Add(root);
			if (maxCount >= 0 && nodes.Count >= maxCount)
				return;

			if (root == null)
				return;

			foreach (var child in root.GetChildren())
			{
				BFS(child, nodes, maxCount);
				if (maxCount >= 0 && nodes.Count >= maxCount)
					return;
			}
		}
	}
}
