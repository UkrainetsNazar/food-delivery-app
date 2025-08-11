using Contracts.Enums;
using Microsoft.EntityFrameworkCore;
using Restaurant.API.Data;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Domain.Services;

public class RestaurantDomainService(RestaurantDbContext context)
{
    private readonly RestaurantDbContext _context = context;

    public async Task<SupportedRestaurant> CreateRestaurantAsync(SupportedRestaurant restaurant)
    {
        if (restaurant.Id == Guid.Empty)
            restaurant.Id = Guid.NewGuid();

        await _context.Restaurants.AddAsync(restaurant);
        await _context.SaveChangesAsync();
        return restaurant;
    }

    public async Task<SupportedRestaurant?> UpdateRestaurantAsync(SupportedRestaurant updatedRestaurant)
    {
        var existing = await _context.Restaurants.FindAsync(updatedRestaurant.Id);
        if (existing == null)
            return null;

        existing.Name = updatedRestaurant.Name;
        existing.Description = updatedRestaurant.Description;
        existing.Address = updatedRestaurant.Address;
        existing.PhoneNumber = updatedRestaurant.PhoneNumber;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteRestaurantAsync(Guid restaurantId)
    {
        var restaurant = await _context.Restaurants.FindAsync(restaurantId);
        if (restaurant == null)
            return false;

        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SupportedRestaurant>> GetAllRestaurantsAsync()
    {
        return await _context.Restaurants.AsNoTracking().ToListAsync();
    }

    public async Task<List<DishCategory>> GetRestaurantCategoriesAsync(Guid restaurantId)
    {
        return await _context.Dishes
            .AsNoTracking()
            .Where(d => d.RestaurantId == restaurantId)
            .Select(d => d.Category)
            .Distinct()
            .ToListAsync();
    }
}

