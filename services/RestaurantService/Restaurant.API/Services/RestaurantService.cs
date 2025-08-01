using Microsoft.EntityFrameworkCore;
using Restaurant.API.Data;
using Restaurant.API.Domain.Entities;
using Restaurant.API.Interfaces;

namespace Restaurant.API.Services;

public class RestaurantService(RestaurantDbContext context) : IRestaurantService
{
    private readonly RestaurantDbContext _context = context;

    public async Task<SupportedRestaurant> CreateRestaurantAsync(SupportedRestaurant restaurant)
    {
        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        return restaurant;
    }

    public async Task<SupportedRestaurant?> UpdateRestaurantAsync(SupportedRestaurant restaurant)
    {
        var existingRestaurant = await _context.Restaurants.FindAsync(restaurant.Id);
        if (existingRestaurant is null) return null;

        _context.Entry(existingRestaurant).CurrentValues.SetValues(restaurant);
        await _context.SaveChangesAsync();
        return restaurant;
    }

    public async Task<bool> DeleteRestaurantAsync(Guid restaurantId)
    {
        var restaurant = await _context.Restaurants.FindAsync(restaurantId);
        if (restaurant is null) return false;

        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<SupportedRestaurant>> GetAllRestaurantsAsync()
    {
        return await _context.Restaurants.AsNoTracking().ToListAsync();
    }
}