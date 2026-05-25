using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Persistence;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductId).IsRequired().HasMaxLength(100);
            entity.Property(i => i.ProductName).IsRequired().HasMaxLength(255);
            entity.HasIndex(i => i.ProductId).IsUnique();
        });

        // Seed data for testing
        modelBuilder.Entity<InventoryItem>().HasData(
            new { Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"), ProductId = "prod-1", ProductName = "Widget A", AvailableQuantity = 100, ReservedQuantity = 0, UpdatedAt = DateTime.UtcNow },
            new { Id = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"), ProductId = "prod-2", ProductName = "Widget B", AvailableQuantity = 50, ReservedQuantity = 0, UpdatedAt = DateTime.UtcNow }
        );
    }
}
