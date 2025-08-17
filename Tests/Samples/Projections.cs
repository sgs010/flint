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
	}
}
