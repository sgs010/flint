using Microsoft.EntityFrameworkCore;

namespace Samples
{
	public class User
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
	}

	public class Order
	{
		public int Id { get; set; }
		public int Number { get; set; }
		public ICollection<OrderItem> Items { get; set; }
	}

	public class OrderItem
	{
		public int Id { get; set; }
		public Product Product { get; set; }
	}

	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public decimal Price { get; set; }
	}

	public class Todo
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public User User { get; set; }
		public bool IsCompleted { get; set; }
	}

	public class Blog
	{
		public int Id { get; set; }
		public ICollection<Post> Posts { get; set; }
		public ICollection<Tag> Tags { get; set; }
	}

	public class Post
	{
		public int Id { get; set; }
		public int BlogId { get; set; }
		public Blog Blog { get; set; }
		public string Text { get; set; }
		public User Author { get; set; }
	}

	public class Tag
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	public class Outbox
	{
		public int Id { get; set; }
		public string Message { get; set; }
		public bool IsProcessed { get; set; }
	}

	public class DB : DbContext
	{
		public DbSet<User> Users => Set<User>();
		public DbSet<Order> Orders => Set<Order>();
		public DbSet<OrderItem> OrderItems => Set<OrderItem>();
		public DbSet<Product> Products => Set<Product>();
		public DbSet<Todo> Todos => Set<Todo>();
		public DbSet<Blog> Blogs => Set<Blog>();
		public DbSet<Post> Posts => Set<Post>();
		public DbSet<Tag> Tags => Set<Tag>();
		public DbSet<Outbox> Outbox => Set<Outbox>();
	}
}
