using Microsoft.EntityFrameworkCore;

namespace Samples
{
	public class User
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
	}

	public class DB : DbContext
	{
		public DbSet<User> Users => Set<User>();
	}
}
