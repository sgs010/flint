using System.Text;
using Mono.Cecil;

namespace Flint.Common
{
	static class PrettyPrintExtensions
	{
		public static void PrettyPrint<T, U>(this IEnumerable<T> col, StringBuilder sb, string separator, Func<T, U> selector)
		{
			var needSeparator = false;
			foreach (var x in col)
			{
				if (needSeparator)
					sb.Append(separator);
				sb.Append(selector(x));
				needSeparator = true;
			}
		}

		public static void PrettyPrint(this MethodDefinition mtd, StringBuilder sb)
		{
			sb.Append(mtd.DeclaringType.Namespace);
			sb.Append('.');
			sb.Append(mtd.DeclaringType.Name);
			sb.Append('.');
			sb.Append(mtd.Name);
			sb.Append("()");
		}
	}
}
