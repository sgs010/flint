using Microsoft.EntityFrameworkCore;

namespace WebApp
{
	public class User
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
	}

	public class Todo
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public User User { get; set; }
		public bool IsCompleted { get; set; }
	}

	public class DB : DbContext
	{
		public DbSet<User> Users => Set<User>();
		public DbSet<Todo> Todos => Set<Todo>();
	}
}
