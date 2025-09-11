using Mono.Cecil.Cil;

namespace Flint.Vm
{
	abstract class Ast : IEquatable<Ast>
	{
		public readonly SequencePoint Debug;
		protected Ast(SequencePoint debug)
		{
			Debug = debug;
		}

		public abstract IEnumerable<Ast> GetChildren();
		public abstract bool Equals(Ast other);
		public virtual void Capture(Ast other, IDictionary<string, Ast> captures) { }

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
			var patternNodes = Traverse(pattern, -1);
			var rootNodes = Traverse(root, patternNodes.Count);
			for (var i = 0; i < patternNodes.Count; ++i)
			{
				var p = patternNodes[i];
				var r = rootNodes[i];
				p.Capture(r, captures);
			}
		}

		private static List<Ast> Traverse(Ast root, int maxCount)
		{
			var nodes = new List<Ast>(maxCount > 0 ? maxCount : 0);
			Traverse(root, nodes, maxCount);
			return nodes;
		}

		private static void Traverse(Ast root, List<Ast> nodes, int maxCount)
		{
			if (root == null)
				return;
			if (maxCount == 0)
				return;

			nodes.Add(root);
			--maxCount;
			if (maxCount == 0)
				return;

			foreach (var child in root.GetChildren())
			{
				Traverse(child, nodes, maxCount);
				--maxCount;
				if (maxCount == 0)
					return;
			}
		}
	}

	static class AstExtensions
	{
		public static void Capture(this Ast[] col, Ast[] other, IDictionary<string, Ast> captures)
		{
			if (col.Length != other.Length)
				throw new InvalidOperationException();

			for (var i = 0; i < col.Length; ++i)
				col[i].Capture(other[i], captures);
		}
	}
}
