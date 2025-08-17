namespace Flint.Common
{
	static class CollectionExtensions
	{
		public static void RemoveAll<T>(this HashSet<T> set, IEnumerable<T> values)
		{
			foreach (var x in values)
				set.Remove(x);
		}
	}
}
