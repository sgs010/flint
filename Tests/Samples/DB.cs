using Microsoft.EntityFrameworkCore;

namespace Samples
{
	public class User
	{
		public int Id { get; set; }
		public string Name { get; set; }
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

	public class DB : DbContext
	{
		public DbSet<User> Users => Set<User>();
		public DbSet<Order> Orders => Set<Order>();
		public DbSet<OrderItem> OrderItems => Set<OrderItem>();
		public DbSet<Product> Products => Set<Product>();
	}
}
