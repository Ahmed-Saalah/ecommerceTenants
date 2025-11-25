using Microsoft.EntityFrameworkCore;
using Store.Core.Models;

namespace Store.Core.DbContexts;

internal class StoreDbContext : DbContext
{
    public StoreDbContext(DbContextOptions<StoreDbContext> options)
        : base(options) { }

    public DbSet<Models.Store> Customers { get; set; }
    public DbSet<Document> Addresses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.Store>(entity =>
        {
            entity.HasKey(s => s.StoreId);

            entity
                .HasMany(s => s.Documents)
                .WithOne(d => d.Store)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(a => a.DocumentId);
        });
    }
}
