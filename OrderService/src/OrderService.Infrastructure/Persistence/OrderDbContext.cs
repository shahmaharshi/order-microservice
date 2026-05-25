using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;

namespace OrderService.Infrastructure.Persistence;

public class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CustomerId).IsRequired().HasMaxLength(100);
            entity.Property(o => o.CustomerEmail).IsRequired().HasMaxLength(255);
            entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
            entity.Property(o => o.Status).HasConversion<string>();
            entity.Property(o => o.RowVersion).IsRowVersion();

            entity.HasMany(o => o.Items)
                  .WithOne()
                  .HasForeignKey(i => i.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(o => o.CustomerId);
            entity.HasIndex(o => o.Status);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.ProductId).IsRequired().HasMaxLength(100);
            entity.Property(i => i.ProductName).IsRequired().HasMaxLength(255);
            entity.Property(i => i.UnitPrice).HasColumnType("decimal(18,2)");
        });
    }
}
