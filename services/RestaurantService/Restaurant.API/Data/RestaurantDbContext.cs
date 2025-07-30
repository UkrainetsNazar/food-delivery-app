using Microsoft.EntityFrameworkCore;
using RestaurantService.Domain.Entities;

namespace RestaurantService.Data;

public class RestaurantDbContext : DbContext
{
    public RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : base(options){  }

    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<DishCategory> DishCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Restaurant>()
            .HasMany(r => r.Dishes)
            .WithOne(d => d.Restaurant)
            .HasForeignKey(d => d.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DishCategory>()
            .HasMany(c => c.Dishes)
            .WithOne(d => d.DishCategory)
            .HasForeignKey(d => d.DishCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Restaurant>()
            .HasMany(r => r.DishCategories)
            .WithMany(c => c.Restaurants);
    }
}
