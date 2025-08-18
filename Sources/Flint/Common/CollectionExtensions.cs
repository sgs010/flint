namespace Flint.Common
{
	static class CollectionExtensions
	{
		public static void RemoveAll<T>(this HashSet<T> set, IEnumerable<T> values)
		{
			foreach (var x in values)
				set.Remove(x);
		}

		public static void AddOrReplace<K, V>(this Dictionary<K, V> dic, K key, V value)
		{
			if (dic.TryAdd(key, value) == false)
				dic[key] = value;
		}
	}
}
