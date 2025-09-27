using Microsoft.EntityFrameworkCore;

namespace Samples
{
	interface IRepository
	{
		DbSet<Blog> Blogs { get; }
		DbSet<Post> Posts { get; }
		DbSet<Outbox> Outbox { get; }

		Task SaveChangesAsync();
	}

	class Repository : IRepository
	{
		private readonly DB _db;
		public Repository(DB db)
		{
			_db = db;
		}

		public DbSet<Blog> Blogs => _db.Blogs;
		public DbSet<Post> Posts => _db.Posts;
		public DbSet<Outbox> Outbox => _db.Outbox;

		public Task SaveChangesAsync()
		{
			return _db.SaveChangesAsync();
		}
	}
}
