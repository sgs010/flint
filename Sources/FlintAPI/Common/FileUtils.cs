using System.Security.Cryptography;
using System.Text;

namespace Flint.Common
{
	static class FileUtils
	{
		public static string GetSHA256Hash(string path)
		{
			using var fs = File.OpenRead(path);
			return GetSHA256Hash(fs);
		}

		public static string GetSHA256Hash(Stream stream)
		{
			using var sha256 = SHA256.Create();
			var hashBytes = sha256.ComputeHash(stream);

			if (stream.CanSeek)
				stream.Position = 0;

			var sb = new StringBuilder();
			foreach (byte b in hashBytes)
				sb.Append(b.ToString("x2"));
			return sb.ToString();
		}
	}
}
