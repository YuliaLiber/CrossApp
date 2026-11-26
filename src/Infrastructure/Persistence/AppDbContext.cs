using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<OrderRow> Orders => Set<OrderRow>();
    public DbSet<OrderLineRow> OrderLines => Set<OrderLineRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var order = modelBuilder.Entity<OrderRow>();

        order.ToTable("Orders");
        order.HasKey(x => x.Id);
        order.Property(x => x.Id).HasMaxLength(32);
        order.Property(x => x.CustomerId)
            .IsRequired().HasMaxLength(100);
        order.Property(x => x.Status)
            .IsRequired().HasMaxLength(20);
        order.Property(x => x.CreatedAt).IsRequired();

        order.HasMany(x => x.Lines)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        var line = modelBuilder.Entity<OrderLineRow>();

        line.ToTable("OrderLines");
        line.HasKey(x => x.Id);
        line.Property(x => x.OrderId)
            .IsRequired().HasMaxLength(32);
        line.Property(x => x.ProductId)
            .IsRequired().HasMaxLength(100);
        line.Property(x => x.Name)
            .IsRequired().HasMaxLength(200);
        line.Property(x => x.Price)
            .HasPrecision(18, 2).IsRequired();
        line.Property(x => x.Quantity).IsRequired();
    }
}
