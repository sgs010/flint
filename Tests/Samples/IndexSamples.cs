using Microsoft.EntityFrameworkCore;

namespace Samples
{
	public static class IndexSamples
	{
		public static async void OrderBy()
		{
			// should advise index on Email column

			using var db = new DB();
			var users = await db.Users
				.AsNoTracking()
				.OrderBy(u => u.Email)
				.ToListAsync();

			foreach (var user in users)
				Console.WriteLine(user);
		}

		public static async void Where()
		{
			// should advise index on Email column

			using var db = new DB();
			var users = await db.Users
				.AsNoTracking()
				.Where(u => u.Email != null)
				.ToListAsync();

			foreach (var user in users)
				Console.WriteLine(user);
		}

		public static async void FirstOrDefaultAsync()
		{
			// should advise index on Email column

			using var db = new DB();
			var user = await db.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Email == "test");
			Console.WriteLine(user);
		}

		public static async void PrimaryKeyDefault()
		{
			// do not advise index on Id

			using var db = new DB();
			var user = await db.Users
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.Id == 42);
			Console.WriteLine(user);
		}

		public static async void PrimaryKeyFromKeyAttribute()
		{
			// do not advise index on UserId

			using var db = new DB();
			var user = await db.UserDataPKKey
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.UserId == 42);
			Console.WriteLine(user);
		}

		public static async void PrimaryKeyFromPrimaryKeyAttribute()
		{
			// do not advise index on UserId

			using var db = new DB();
			var user = await db.UserDataPKPrimaryKey
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.UserId == 42);
			Console.WriteLine(user);
		}

		public static async void PrimaryKeyFromFluent()
		{
			// do not advise index on UserId

			using var db = new DB();
			var user = await db.UserDataPKFluent
				.AsNoTracking()
				.FirstOrDefaultAsync(u => u.UserId == 42);
			Console.WriteLine(user);
		}

		public static async void Mixed()
		{
			// should advise index on User.Email
			// should advise index on Product.Name

			using var db = new DB();
			var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "test");
			Console.WriteLine(user);
			var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Name == "qwerty");
			Console.WriteLine(product);
		}

		public static async void NestedChain()
		{
			// should advise index on Product.Price

			using var db = new DB();
			var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Items.Any(i => i.Product.Price > 20));
			Console.WriteLine(order);
		}

		public static async void NestedAny()
		{
			// should advise index on OrderItem.Total

			using var db = new DB();
			var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Items.Any(i => i.Total < 100));
			Console.WriteLine(order);
		}
	}
}
