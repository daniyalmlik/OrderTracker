using Microsoft.EntityFrameworkCore;
using OrderTracker.Api.Domain.Entities;

namespace OrderTracker.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderEvent> OrderEvents => Set<OrderEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).HasMaxLength(512).IsRequired();
            entity.Property(u => u.FullName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.RefreshToken).HasMaxLength(512);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(o => o.Id);
            entity.Property(o => o.ItemName).HasMaxLength(200).IsRequired();
            entity.Property(o => o.Quantity).HasDefaultValue(1);
            // Store status as string so it's readable in the DB
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(o => o.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(o => o.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(o => o.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasConversion<string>().HasMaxLength(100);
            entity.Property(e => e.EventData).IsRequired();
            entity.Property(e => e.IdempotencyKey).HasMaxLength(256).IsRequired();
            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.Events)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
