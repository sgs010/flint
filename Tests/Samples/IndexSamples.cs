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

		public static async void NestedWhere()
		{
			// should advise index on Email and FirstName columns

			using var db = new DB();
			var users = await db.Users
				.AsNoTracking()
				.Where(u => u.Email != null)
				.Where(u => u.FirstName.StartsWith('A'))
				.ToListAsync();

			foreach (var user in users)
				Console.WriteLine(user);
		}

		public static async void FirstAsync()
		{
			// should advise index on Email column

			using var db = new DB();
			var user = await db.Users
				.AsNoTracking()
				.FirstAsync(u => u.Email == "test");
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

		public static async void LastAsync()
		{
			// should advise index on Email column

			using var db = new DB();
			var user = await db.Users
				.AsNoTracking()
				.LastAsync(u => u.Email == "test");
			Console.WriteLine(user);
		}

		public static async void LastOrDefaultAsync()
		{
			// should advise index on Email column

			using var db = new DB();
			var user = await db.Users
				.AsNoTracking()
				.LastOrDefaultAsync(u => u.Email == "test");
			Console.WriteLine(user);
		}

		public static async void SingleAsync()
		{
			// should advise index on Email column

			using var db = new DB();
			var user = await db.Users
				.AsNoTracking()
				.SingleAsync(u => u.Email == "test");
			Console.WriteLine(user);
		}

		public static async void SingleOrDefaultAsync()
		{
			// should advise index on Email column

			using var db = new DB();
			var user = await db.Users
				.AsNoTracking()
				.SingleOrDefaultAsync(u => u.Email == "test");
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
			// should advise index on Users.Email
			// should advise index on Products.Name

			using var db = new DB();
			var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "test");
			Console.WriteLine(user);
			var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Name == "qwerty");
			Console.WriteLine(product);
		}

		public static async void NestedChain()
		{
			// should advise index on Products.Price

			using var db = new DB();
			var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Items.Any(i => i.Product.Price > 20));
			Console.WriteLine(order);
		}

		public static async void NestedAny()
		{
			// should advise index on OrderItems.Total

			using var db = new DB();
			var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Items.Any(i => i.Total < 100));
			Console.WriteLine(order);
		}

		public static async void MixedWhere()
		{
			// should advise index on Todos.IsCompleted
			// should advise index on Users.FirstName

			using var db = new DB();
			var todos = await db.Todos
				.AsNoTracking()
				.Where(t => t.IsCompleted && t.User.FirstName.StartsWith('A'))
				.ToListAsync();

			foreach (var todo in todos)
				Console.WriteLine(todo);
		}

		public static async void OrderByDescending()
		{
			// should advise index on Email column

			using var db = new DB();
			var users = await db.Users
				.AsNoTracking()
				.OrderByDescending(u => u.Email)
				.ToListAsync();

			foreach (var user in users)
				Console.WriteLine(user);
		}

		public static async void FilteredInclude()
		{
			// should advise index on columns (CreatedDate,TotalAmount) of table Orders2

			using var db = new DB();
			var usersWithRecentBigOrders = await db.Users2
				.Include(u => u.Orders
					.Where(o => o.TotalAmount > 500 && o.CreatedDate > DateTime.Now.AddDays(-30))
					.OrderByDescending(o => o.TotalAmount))
				.AsNoTracking()
				.ToListAsync();

			foreach (var user in usersWithRecentBigOrders)
				Console.WriteLine(user);
		}
	}
}
