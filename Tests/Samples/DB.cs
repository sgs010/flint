using Microsoft.EntityFrameworkCore;

namespace Samples
{
	public class User
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime BirthDate { get; set; }
		public string Address { get; set; }
	}

	public class DB : DbContext
	{
		public DbSet<User> Users => Set<User>();
	}
}
