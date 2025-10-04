using System.Text;

namespace FlintTests
{
	static class AssertExtensions
	{
		#region Interface
		public static void AssertNotEmpty<T>(this IEnumerable<T> col)
		{
			if (col.Any() == false)
				Assert.Fail("Collection is empty.");
		}

		public static void AssertEmpty<T>(this IEnumerable<T> col)
		{
			if (col.Any())
				Assert.Fail("Collection is not empty.");
		}

		public static void AssertSame<T>(this IReadOnlyCollection<T> actual, IReadOnlyCollection<T> expected)
		{
			var diffExpected = expected.Except(actual).ToList();
			var diffActual = actual.Except(expected).ToList();

			if (diffExpected.Count == 0 && diffActual.Count == 0)
				return;

			var sb = new StringBuilder();
			sb.AppendLine("Collections are not same.");
			PrettyPrintDifferences(sb, "=== Expected:", diffExpected);
			PrettyPrintDifferences(sb, "=== Actual:", diffActual);
			Assert.Fail(sb.ToString());
		}

		public static void AssertContains<T>(this IReadOnlyCollection<T> col, Func<T, bool> predicate)
		{
			if (col.Any(predicate) == false)
				Assert.Fail("Collection does not contain a required items.");
		}

		public static void AssertCount<T>(this IReadOnlyCollection<T> col, int count)
		{
			if (col.Count != count)
				Assert.Fail($"Collection is expected to have {count} items, got {col.Count}.");
		}

		public static void AssertEquals<T>(this T actual, T expected)
		{
			if (actual.Equals(expected))
				return;

			var sb = new StringBuilder();
			sb.AppendLine("Values are not equal.");
			sb.AppendLine("=== Expected:");
			sb.AppendLine(expected.ToString());
			sb.AppendLine("=== Actual:");
			sb.AppendLine(actual.ToString());
			Assert.Fail(sb.ToString());
		}
		#endregion

		#region Implementation
		private static void PrettyPrintDifferences<T>(StringBuilder sb, string caption, List<T> differences)
		{
			ArgumentNullException.ThrowIfNull(differences);
			sb.AppendLine(caption);
			if (differences.Count == 0)
			{
				sb.AppendLine("...");
			}
			else
			{
				foreach (var x in differences)
					sb.AppendLine(x.ToString());
			}
		}
		#endregion
	}
}
