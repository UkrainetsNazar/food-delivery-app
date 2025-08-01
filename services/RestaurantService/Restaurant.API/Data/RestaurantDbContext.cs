using Microsoft.EntityFrameworkCore;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Data;

public class RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : DbContext(options)
{
    public DbSet<SupportedRestaurant> Restaurants { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<DishCategory> DishCategories { get; set; }
    public DbSet<RestaurantCategory> RestaurantCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<RestaurantCategory>()
            .HasKey(rc => new { rc.RestaurantId, rc.DishCategoryId });

        modelBuilder.Entity<RestaurantCategory>()
            .HasOne(rc => rc.Restaurant)
            .WithMany(r => r.RestaurantCategories)
            .HasForeignKey(rc => rc.RestaurantId);

        modelBuilder.Entity<RestaurantCategory>()
            .HasOne(rc => rc.DishCategory)
            .WithMany(dc => dc.RestaurantCategories)
            .HasForeignKey(rc => rc.DishCategoryId);

        modelBuilder.Entity<Dish>()
            .HasOne(d => d.Restaurant)
            .WithMany(r => r.Dishes)
            .HasForeignKey(d => d.RestaurantId);

        modelBuilder.Entity<Dish>()
            .HasOne(d => d.DishCategory)
            .WithMany(dc => dc.Dishes)
            .HasForeignKey(d => d.DishCategoryId);
    }
}
