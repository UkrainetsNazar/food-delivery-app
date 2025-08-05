using Microsoft.EntityFrameworkCore;
using Restaurant.API.Data;
using Restaurant.API.Domain.Entities;
using Restaurant.API.Interfaces;

namespace Restaurant.API.Services;

public class DishService(RestaurantDbContext context) : IDishService
{
    private readonly RestaurantDbContext _context = context;

    public async Task<Dish> CreateDishAsync(Dish dish)
    {
        await _context.AddAsync(dish);
        await _context.SaveChangesAsync();

        return dish;
    }

    public async Task<bool> DeleteDishAsync(Guid dishId)
    {
        var dish = await _context.Dishes.FindAsync(dishId);
        if (dish is null) return false;

        _context.Dishes.Remove(dish);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<Dish>> GetDishesByNameAsync(string name)
    {
        var dishes = await _context.Dishes.AsNoTracking().Where(d => EF.Functions.ILike(d.Name!, name)).ToListAsync();

        return dishes;
    }

    public async Task<Dish?> UpdateDishAsync(Dish dish)
    {
        var existingDish = await _context.Dishes.FindAsync(dish.Id);
        if (existingDish is null) return null;

        _context.Entry(existingDish).CurrentValues.SetValues(dish);
        await _context.SaveChangesAsync();
        return existingDish;
    }

    public async Task<Dish?> GetDishByIdAsync(Guid dishId)
    {
        return await _context.Dishes.AsNoTracking().FirstOrDefaultAsync(d => d.Id == dishId);
    }

    public async Task<IEnumerable<Dish>> GetDishesByRestaurantAsync(Guid restaurantId)
    {
        return await _context.Dishes
            .AsNoTracking()
            .Where(d => d.RestaurantId == restaurantId)
            .ToListAsync();
    }
}