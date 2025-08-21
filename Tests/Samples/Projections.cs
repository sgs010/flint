using Microsoft.EntityFrameworkCore;

namespace Samples
{
	static class Projections
	{
		public static async void Test001()
		{
			using var db = new DB();
			var users = await db.Users.ToListAsync();
			foreach (var user in users)
			{
				Console.WriteLine($"{user.Id} {user.FirstName} {user.LastName}");
			}
		}

		public static async void Test002()
		{
			using var db = new DB();

			var names = await db.Users.ToListAsync();
			foreach (var user in names)
			{
				Console.WriteLine($"{user.Id} {user.FirstName}");
			}

			var addresses = await db.Users.ToListAsync();
			foreach (var user in addresses)
			{
				Console.WriteLine($"{user.Id} {user.Address}");
			}
		}

		public static void Test003()
		{
			using var db = new DB();
			foreach (var user in db.Users)
			{
				Console.WriteLine($"{user.Id} {user.FirstName} {user.LastName}");
			}
		}

	}
}
