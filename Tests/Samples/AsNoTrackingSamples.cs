using Microsoft.EntityFrameworkCore;

namespace Samples
{
	public static class AsNoTrackingSamples
	{
		public static void Read()
		{
			// should advise to add AsNoTracking

			var app = new App();
			app.MapGet("/test", async (DB db) =>
			{
				var todos = await db.Todos.Where(x => x.IsCompleted).ToListAsync();
				return todos.Select(t => new
				{
					Id = t.Id,
					Name = t.Name,
					User = t.User.FirstName + t.User.LastName,
				});
			});
		}

		public static void Update()
		{
			// should not advise to add AsNoTracking because entity is changed

			var app = new App();
			app.MapPost("/test", async (int id, DB db) =>
			{
				var order = await db.Orders.FirstOrDefaultAsync(x => x.Id == id);
				if (order == null)
					return 404;

				foreach (var item in order.Items)
				{
					if (item.Product.Name == "test")
						item.Product.Price = 42;
				}
				await db.SaveChangesAsync();
				return 200;
			});
		}
	}
}
