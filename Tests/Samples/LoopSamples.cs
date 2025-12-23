using Microsoft.EntityFrameworkCore;

namespace Samples
{
	public static class LoopSamples
	{
		public static async void Loop()
		{
			// should advise not to make db queries in a loop

			using var db = new DB();
			for (var i = 0; i < 10; ++i)
			{
				var user = await db.Users.FirstOrDefaultAsync(x => x.Id == i);
				Console.WriteLine(user);
			}
		}

		public static async void NoLoop()
		{
			// should not advise

			using var db = new DB();
			if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
			{
				var user = await db.Users.FirstOrDefaultAsync(x => x.Id == 42);
				Console.WriteLine(user);
			}
		}

		public static async void NotInLoop()
		{
			// should not advise

			using var db = new DB();
			var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == 42);
			foreach (var item in order.Items)
			{
				Console.Write(item);
			}
		}

		public static async void Mixed()
		{
			// should advise not to make db queries in a loop

			using var db = new DB();

			for (var i = 0; i < 10; ++i)
			{
				var user = await db.Users.FirstOrDefaultAsync(x => x.Id == i);
				Console.WriteLine(user);
			}

			var data = await db.Products.Select(x => new { x.Id, x.Name }).ToListAsync();
			foreach (var item in data)
			{
				if (item.Name.Contains("apple"))
				{
					var product = await db.Products.FirstOrDefaultAsync(p => p.Id == item.Id);
					Console.WriteLine(product);
				}
			}
		}

		public static void Lambda()
		{
			// should advise not to make db queries in a loop

			var app = new App();
			app.MapGet("/test", async (DB db) =>
			{
				var data = await db.Todos.Select(x => new { x.IsCompleted, x.Id }).ToListAsync();
				var result = new List<Todo>();
				foreach (var item in data)
				{
					if (item.IsCompleted)
					{
						var todo = await db.Todos.FirstOrDefaultAsync(x => x.Id == item.Id);
						result.Add(todo);
					}
				}
				return result;
			});
		}
	}
}
