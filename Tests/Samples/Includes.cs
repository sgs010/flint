using Microsoft.EntityFrameworkCore;

namespace Samples
{
	public static class Includes
	{
		record struct TodoDto(int Id, string Name, string User);

		public static void Lambda_NestedEntity()
		{
			// should advise Include(t => t.User)

			var app = new App();
			app.MapGet("/todos", async (DB db) =>
			{
				var todos = await db.Todos.Where(x => x.IsCompleted).ToListAsync();
				return todos.Select(t => new TodoDto
				{
					Id = t.Id,
					Name = t.Name,
					User = t.User.FirstName + t.User.LastName,
				});
			});
		}

		public static void Lambda_NoNestedEntities()
		{
			// should not advise any includes

			var app = new App();
			app.MapGet("/todos", async (DB db) =>
			{
				var todos = await db.Todos.Where(x => x.IsCompleted).ToListAsync();
				return todos.Select(t => new TodoDto
				{
					Id = t.Id,
					Name = t.Name,
				});
			});
		}

		public static async void ChainedEntities_NoInclude()
		{
			// should advise Include(o => o.Items).ThenInclude(i => i.Product)

			using var db = new DB();
			var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == 42);
			foreach (var item in order.Items)
			{
				Console.WriteLine($"{item.Product.Name}");
			}
		}

		public static async void ChainedEntities_FullInclude()
		{
			// should not advise any includes

			using var db = new DB();

			var order = await db.Orders
				.Include(o => o.Items)
				.ThenInclude(i => i.Product)
				.FirstOrDefaultAsync(o => o.Id == 42);

			foreach (var item in order.Items)
			{
				Console.WriteLine($"{item.Product.Name}");
			}
		}

		public static async void ChainedEntities_PartialInclude()
		{
			// should advise ThenInclude(i => i.Product)

			using var db = new DB();

			var order = await db.Orders
				.Include(o => o.Items)
				.FirstOrDefaultAsync(o => o.Id == 42);

			foreach (var item in order.Items)
			{
				Console.WriteLine($"{item.Product.Name}");
			}
		}
	}
}
