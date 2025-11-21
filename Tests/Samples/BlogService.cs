namespace Samples
{
	interface IBlogService
	{
		Task<int> AddPost(int blogId, string text);
	}

	class BlogService : IBlogService
	{
		private readonly IRepository _repo;

		public BlogService(IRepository repo)
		{
			_repo = repo;
		}

		public async Task<int> AddPost(int blogId, string text)
		{
			var post = new Post { BlogId = blogId, Text = text };
			_repo.Posts.Add(post);
			await _repo.SaveChangesAsync();
			return post.Id;
		}
	}
}
