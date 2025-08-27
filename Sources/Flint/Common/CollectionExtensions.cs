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

		public static V GetOrAddValue<K, V>(this Dictionary<K, V> dic, K key) where V : class, new()
		{
			if (dic.TryGetValue(key, out var value) == false)
			{
				value = new();
				dic.Add(key, value);
			}
			return value;
		}
	}
}
