using System.Collections.Immutable;
using Flint.Vm;

namespace Flint.Common
{
	static class CollectionExtensions
	{
		public static void RemoveAll<T>(this HashSet<T> set, IEnumerable<T> values)
		{
			foreach (var x in values)
				set.Remove(x);
		}

		public static void AddOrReplace<K, V>(this IDictionary<K, V> dic, K key, V value)
		{
			if (dic.TryAdd(key, value) == false)
				dic[key] = value;
		}

		public static V GetOrAddValue<K, V>(this IDictionary<K, V> dic, K key) where V : class, new()
		{
			if (dic.TryGetValue(key, out var value) == false)
			{
				value = new();
				dic.Add(key, value);
			}
			return value;
		}

		public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
		{
			return collection == null || collection.Count == 0;
		}

		public static ImmutableArray<T> Concat<T>(this ImmutableArray<T> head, IReadOnlyCollection<ImmutableArray<T>> tail)
		{
			var totalLength = head.Length + tail.Sum(x => x.Length);
			var builder = ImmutableArray.CreateBuilder<T>(totalLength);
			builder.AddRange(head);
			foreach (var x in tail)
				builder.AddRange(x);
			return builder.ToImmutable();
		}

		public static Stack<T> Clone<T>(this Stack<T> stack)
		{
			var items = stack.ToArray();
			System.Array.Reverse(items);
			return new Stack<T>(items);
		}

#if NET48
		public static U GetValueOrDefault<T, U>(this IReadOnlyDictionary<T, U> dic, T key)
		{
			if (dic.TryGetValue(key, out var value))
				return value;
			return default;
		}

		public static bool TryAdd<T, U>(this IDictionary<T, U> dic, T key, U value)
		{
			if (dic.ContainsKey(key))
				return false;

			dic.Add(key, value);
			return true;
		}
#endif
	}
}
