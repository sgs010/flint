using Microsoft.EntityFrameworkCore;

namespace Samples
{
	public static class OutboxSamples
	{
		public static void NoOutbox()
		{
			// should advise to use Outbox pattern

			var app = new App();
			app.MapPost("/posts/{id}/{text}", async (int id, string text, IRepository repo, IEventBus bus) =>
			{
				var post = new Post { BlogId = id, Text = text };
				repo.Posts.Add(post);
				await repo.SaveChangesAsync();

				await bus.PublishAsync($"new post {post.Id}");

				return 200;
			});
		}

		public static void DelayedOutbox()
		{
			// should not advise Outbox

			var app = new App();
			app.MapPost("/posts/{id}/{text}", async (int id, string text, IRepository repo, IEventBus bus) =>
			{
				repo.Posts.Add(new Post { BlogId = id, Text = text });
				repo.Outbox.Add(new Outbox { Message = "new post" });
				await repo.SaveChangesAsync();
				return 200;
			});
		}

		public static void ImmediateOutbox()
		{
			// should not advise Outbox

			var app = new App();
			app.MapPost("/posts/{id}/{text}", async (int id, string text, IRepository repo, IEventBus bus) =>
			{
				repo.Posts.Add(new Post { BlogId = id, Text = text });
				repo.Outbox.Add(new Outbox { Message = "new post" });
				await repo.SaveChangesAsync();

				await bus.PublishAsync($"new post");

				return 200;
			});
		}

		public static void Services()
		{
			// should advise to use Outbox pattern

			var app = new App();
			app.MapPost("/posts/{blogId}/{text}", async (int blogId, string text, IBlogService blogs, IEventService events) =>
			{
				var postId = await blogs.AddPost(blogId, text);
				await events.FirePostAdded(postId);
				return 200;
			});
		}

		public static async Task ProcessOutbox(IRepository repo, IEventBus bus)
		{
			// should not advise Outbox

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
