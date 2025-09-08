using Microsoft.EntityFrameworkCore;

namespace WebApp
{
	public class Program
	{
		record struct TodoDto(int Id, string Name, string User);

		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			builder.Services.AddDbContext<DB>(options => options.UseInMemoryDatabase("test"));

			var app = builder.Build();

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

			app.MapPost("/todos/{id}", async (int id, DB db) =>
			{
				var todo = await db.Todos.FirstOrDefaultAsync(x => x.Id == id);
				if (todo == null)
					return Results.NotFound();

				if (todo.IsCompleted == false)
				{
					todo.IsCompleted = true;
					await db.SaveChangesAsync();
				}
				return Results.NoContent();
			});

			app.Run();
		}
	}
}
