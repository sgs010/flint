using Microsoft.EntityFrameworkCore;

namespace Samples
{
	public static class AsSplitQuery
	{
		public static async void MultipleChains()
		{
			// should advise to add AsSplitQuery

			using var db = new DB();
			var blog = await db.Blogs.FirstOrDefaultAsync(b => b.Id == 42);
			foreach (var post in blog.Posts)
			{
				Console.WriteLine($"{post.Author.FirstName} {post.Author.LastName}");
			}
			foreach (var tag in blog.Tags)
			{
				Console.WriteLine($"{tag.Name}");
			}
		}

		public static async void MultipleChainsWithSplit()
		{
			// should not advise to add AsSplitQuery because it is already present

			using var db = new DB();
			var blog = await db.Blogs.AsSplitQuery().FirstOrDefaultAsync(b => b.Id == 42);
			foreach (var post in blog.Posts)
			{
				Console.WriteLine($"{post.Author.FirstName} {post.Author.LastName}");
			}
			foreach (var tag in blog.Tags)
			{
				Console.WriteLine($"{tag.Name}");
			}
		}

		public static async void SingleChain()
		{
			// should not advise to add AsSplitQuery because there is only one chain

			using var db = new DB();
			var blog = await db.Blogs.FirstOrDefaultAsync(b => b.Id == 42);
			foreach (var post in blog.Posts)
			{
				Console.WriteLine($"{post.Author.FirstName} {post.Author.LastName}");
			}
		}
	}
}
