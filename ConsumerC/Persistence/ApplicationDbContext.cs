﻿using ConsumerC.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConsumerC.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<City> Cities { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Flight> Flights { get; set; }
}
