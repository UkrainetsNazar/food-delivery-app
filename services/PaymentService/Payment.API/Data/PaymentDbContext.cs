namespace Payment.API.Data;

using Microsoft.EntityFrameworkCore;
using Payment.API.Entities;

public class PaymentDbContext(DbContextOptions<PaymentDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>()
            .HasKey(p => p.Id);

        modelBuilder.Entity<Payment>()
            .Property(p => p.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }
}