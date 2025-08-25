using Microsoft.EntityFrameworkCore;

namespace Samples
{
	static class Projections
	{
		public static async void ReadWholeObject()
		{
			// should not advise a projection because we possibly read all object properties

			using var db = new DB();
			var users = await db.Users.ToListAsync();
			foreach (var user in users)
			{
				Console.WriteLine(user);
			}
		}

		public static async void ReadAllProperties()
		{
			// should not advise a projection because we read all object properties

			using var db = new DB();
			var users = await db.Users.ToListAsync();
			foreach (var user in users)
			{
				Console.WriteLine($"{user.Id} {user.Name} {user.Email}");
			}
		}

		public static async void ReadSomeProperties()
		{
			// should advise a projection { Id, Name }

			using var db = new DB();
			var users = await db.Users.ToListAsync();
			foreach (var user in users)
			{
				Console.WriteLine($"{user.Id} {user.Name}");
			}
		}

		public static async void MultipleQueries()
		{
			// should advise a projection per query

			using var db = new DB();

			var usersByName = await db.Users.Where(x => x.Name == "John").ToListAsync();
			foreach (var user in usersByName)
			{
				Console.WriteLine($"{user.Id} {user.Name}");
			}

			var usersByEmail = await db.Users.Where(x => x.Email.StartsWith("test")).ToListAsync();
			foreach (var user in usersByEmail)
			{
				Console.WriteLine($"{user.Id} {user.Email}");
			}
		}

		public static async void ComplexProjection()
		{
			// should advise a complex projection

			using var db = new DB();

			var order = await db.Orders
				.Include(o => o.Items)
				.ThenInclude(i => i.Product)
				.FirstOrDefaultAsync(o => o.Id == 42);

			Console.WriteLine($"{order.Number}");
			foreach (var item in order.Items)
			{
				Console.WriteLine($"{item.Product.Name}");
			}
		}

		public static async void ReadAllChanedProperties()
		{
			// should not advise a projection because all properties are read

			using var db = new DB();

			var order = await db.Orders
				.Include(o => o.Items)
				.ThenInclude(i => i.Product)
				.FirstOrDefaultAsync(o => o.Id == 42);

			Console.WriteLine($"{order.Id} {order.Number}");
			foreach (var item in order.Items)
			{
				Console.WriteLine($"{item.Id} {item.Product.Id} {item.Product.Name}");
			}
		}

		public static async void SimpleCRUD()
		{
			// simple crud by copilot
			// should advise one projection { Name, Price } for READ operation

			using var db = new DB();

			// CREATE
			db.Products.Add(new Product { Name = "Laptop", Price = 1200.00m });
			db.Products.Add(new Product { Name = "Mouse", Price = 25.50m });
			await db.SaveChangesAsync();

			// READ
			var products = await db.Products.ToListAsync();
			Console.WriteLine("Products:");
			foreach (var product in products)
			{
				// should suggest a projection { Name, Price }
				Console.WriteLine($"- {product.Name}: ${product.Price}");
			}

			// UPDATE
			var laptop = await db.Products.FirstAsync(p => p.Name == "Laptop");
			laptop.Price = 1100.00m;
			await db.SaveChangesAsync();

			// DELETE
			var mouse = await db.Products.FirstAsync(p => p.Name == "Mouse");
			db.Products.Remove(mouse);
			await db.SaveChangesAsync();

			// FINAL READ
			Console.WriteLine("\nUpdated Products:");
			foreach (var product in await db.Products.ToListAsync())
			{
				Console.WriteLine(product);
			}
		}

		public static async void ToListAsync()
		{
			// should advise a projection { Name }

			using var db = new DB();
			var users = await db.Users.ToListAsync();
			foreach (var user in users)
			{
				Console.WriteLine($"{user.Name}");
			}
		}

		public static async void ToArrayAsync()
		{
			// should advise a projection { Name }

			using var db = new DB();
			var users = await db.Users.ToArrayAsync();
			foreach (var user in users)
			{
				Console.WriteLine($"{user.Name}");
			}
		}

		public static async void ToHashSetAsync()
		{
			// should advise a projection { Name }

			using var db = new DB();
			var users = await db.Users.ToHashSetAsync();
			foreach (var user in users)
			{
				Console.WriteLine($"{user.Name}");
			}
		}

		public static async void ToDictionaryAsync()
		{
			// should advise a projection { Id, Name }

			using var db = new DB();
			var users = await db.Users.ToDictionaryAsync(x => x.Id);
			foreach (var (_, user) in users)
			{
				Console.WriteLine($"{user.Name}");
			}
		}

		public static async void FirstAsync()
		{
			// should advise a projection { Name }

			using var db = new DB();
			var user = await db.Users.FirstAsync(x => x.Id == 42);
			Console.WriteLine($"{user.Name}");
		}

		public static async void FirstOrDefaultAsync()
		{
			// should advise a projection { Name }

			using var db = new DB();
			var user = await db.Users.FirstOrDefaultAsync(x => x.Id == 42);
			if (user != null)
				Console.WriteLine($"{user.Name}");
		}

		public static async void LastAsync()
		{
			// should advise a projection { Name }

			using var db = new DB();
			var user = await db.Users.LastAsync(x => x.Id == 42);
			Console.WriteLine($"{user.Name}");
		}

		public static async void LastOrDefaultAsync()
		{
			// should advise a projection { Name }

			using var db = new DB();
			var user = await db.Users.LastOrDefaultAsync(x => x.Id == 42);
			Console.WriteLine($"{user.Name}");
		}

		public static async void SingleAsync()
		{
			// should advise a projection { Name }

			using var db = new DB();
			var user = await db.Users.SingleAsync(x => x.Id == 42);
			Console.WriteLine($"{user.Name}");
		}

		public static async void SingleOrDefaultAsync()
		{
			// should advise a projection { Name }

			using var db = new DB();
			var user = await db.Users.SingleOrDefaultAsync(x => x.Id == 42);
			if (user != null)
				Console.WriteLine($"{user.Name}");
		}
	}
}
