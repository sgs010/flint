using System;
using System.Collections.Generic;

namespace FlintVSIX
{
	static class Extensions
	{
		public static List<U> ToList<T, U>(this IReadOnlyCollection<T> col, Func<T, U> convert)
		{
			if (col == null)
				return null;
			if (col.Count == 0)
				return [];

			var result = new List<U>(col.Count);
			foreach (var x in col)
			{
				result.Add(convert(x));
			}
			return result;
		}
	}
}
