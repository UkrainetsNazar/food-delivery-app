using Microsoft.EntityFrameworkCore;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Data;

public class RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : DbContext(options)
{
    public DbSet<SupportedRestaurant> Restaurants { get; set; }
    public DbSet<Dish> Dishes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SupportedRestaurant>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Id).ValueGeneratedOnAdd();
            
            entity.Property(r => r.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(r => r.Description)
                .HasMaxLength(500);
                
            entity.Property(r => r.Address)
                .IsRequired()
                .HasMaxLength(200);
                
            entity.Property(r => r.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);
                
            entity.HasMany(r => r.Dishes)
                .WithOne(d => d.Restaurant)
                .HasForeignKey(d => d.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Dish>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Id).ValueGeneratedOnAdd();
            
            entity.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            entity.Property(d => d.Description)
                .HasMaxLength(500);
                
            entity.Property(d => d.ImageUrl)
                .HasMaxLength(255);
                
            entity.Property(d => d.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();
                
            entity.Property(d => d.Category)
                .HasConversion<string>()
                .IsRequired();
                
            entity.HasOne(d => d.Restaurant)
                .WithMany(r => r.Dishes)
                .HasForeignKey(d => d.RestaurantId)
                .IsRequired();
        });

        modelBuilder.Entity<SupportedRestaurant>()
            .HasIndex(r => r.Name)
            .IsUnique();

        modelBuilder.Entity<Dish>()
            .HasIndex(d => new { d.RestaurantId, d.Name })
            .IsUnique();
    }
}
