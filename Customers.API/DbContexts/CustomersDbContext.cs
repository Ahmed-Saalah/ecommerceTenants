﻿using Customers.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Customers.API.DbContexts;

public class CustomersDbContext : DbContext
{
    public CustomersDbContext(DbContextOptions<CustomersDbContext> options)
        : base(options) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<CustomerTenant> CustomerTenants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Customer config
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(c => c.CustomerId);

            entity.Property(c => c.CustomerId).ValueGeneratedNever();

            entity
                .HasMany(c => c.Addresses)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Address config
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(a => a.AddressId);
        });

        // CustomerTenant config
        modelBuilder.Entity<CustomerTenant>(entity =>
        {
            entity.HasKey(ct => new { ct.CustomerId, ct.TenantId });

            entity
                .HasOne(ct => ct.Customer)
                .WithMany(c => c.CustomerTenants)
                .HasForeignKey(ct => ct.CustomerId);
        });
    }
}
