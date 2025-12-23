using Microsoft.AspNetCore.Mvc;
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

			app.MapGet("/blog/{id}", async (int id, DB db) =>
			{
				var blog = await db.Blogs.FirstOrDefaultAsync(b => b.Id == id);
				return new
				{
					Posts = blog.Posts.Select(post => new { post.Author.FirstName, post.Author.LastName }),
					Tags = blog.Tags.Select(tag => tag.Name)
				};
			});

			app.MapPost("/post/{id}", async (int id, [FromBody] string text, IRepository repo, IEventBus bus) =>
			{
				var post = new Post { BlogId = id, Text = text };
				repo.Posts.Add(post);
				await repo.SaveChangesAsync();

				await bus.PublishAsync($"new post {post.Id}");

				return Results.NoContent();
			});

			app.MapGet("/completedtodos", async (DB db) =>
			{
				var data = await db.Todos.Select(x => new { x.IsCompleted, x.Id }).ToListAsync();
				var result = new List<Todo>();
				foreach (var item in data)
				{
					if (item.IsCompleted)
					{
						var todo = await db.Todos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == item.Id);
						result.Add(todo);
					}
				}
				return result;
			});

			app.Run();
		}

		internal static async Task ProcessOutbox(IRepository repo, IEventBus bus)
		{
			var messages = await repo.Outbox.Where(m => m.IsProcessed == false).Take(10).ToListAsync();
			foreach (var msg in messages)
			{
				await bus.PublishAsync(msg.Message);
				msg.IsProcessed = true;
			}
			await repo.SaveChangesAsync();
		}
	}
}
