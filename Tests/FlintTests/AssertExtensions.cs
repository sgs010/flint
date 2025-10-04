using System.Text;

namespace FlintTests
{
	static class AssertExtensions
	{
		#region Interface
		public static void AssertNotEmpty<T>(this IEnumerable<T> actual)
		{
			if (actual.Any() == false)
				Assert.Fail("Collection is empty.");
		}

		public static void AssertEmpty<T>(this IEnumerable<T> actual)
		{
			if (actual.Any())
				Assert.Fail("Collection is not empty.");
		}

		public static void AssertSame<T>(this ICollection<T> actual, ICollection<T> expected)
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

		public static void AssertContains<T>(this ICollection<T> actual, Func<T, bool> predicate)
		{
			if (actual.Any(predicate) == false)
				Assert.Fail("Collection does not contain a required items.");
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
