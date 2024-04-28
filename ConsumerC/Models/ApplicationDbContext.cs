using Microsoft.EntityFrameworkCore;

namespace ConsumerC.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Flight> Flights { get; set; }
}
