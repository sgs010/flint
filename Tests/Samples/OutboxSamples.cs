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
	}
}
