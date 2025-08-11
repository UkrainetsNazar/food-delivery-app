using Microsoft.EntityFrameworkCore;
using Restaurant.API.Data;
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Domain.Services;

public class DishDomainService(RestaurantDbContext context)
{
    private readonly RestaurantDbContext _context = context;

    public async Task<Dish> CreateDishAsync(Dish dish)
    {
        if (dish.Id == Guid.Empty)
            dish.Id = Guid.NewGuid();

        await _context.Dishes.AddAsync(dish);
        await _context.SaveChangesAsync();
        return dish;
    }

    public async Task<Dish?> UpdateDishAsync(Dish updatedDish)
    {
        var existingDish = await _context.Dishes.FindAsync(updatedDish.Id);
        if (existingDish == null)
            return null;

        existingDish.Name = updatedDish.Name;
        existingDish.Description = updatedDish.Description;
        existingDish.Category = updatedDish.Category;
        existingDish.ImageUrl = updatedDish.ImageUrl;
        existingDish.Price = updatedDish.Price;

        await _context.SaveChangesAsync();

        return existingDish;
    }

    public async Task<bool> DeleteDishAsync(Guid dishId)
    {
        var dish = await _context.Dishes.FindAsync(dishId);
        if (dish == null)
            return false;

        _context.Dishes.Remove(dish);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Dish?> GetDishByIdAsync(Guid dishId)
    {
        return await _context.Dishes.AsNoTracking().FirstOrDefaultAsync(d => d.Id == dishId);
    }

    public async Task<IEnumerable<Dish>> GetDishesByNameAsync(string name)
    {
        return await _context.Dishes
            .AsNoTracking()
            .Where(d => EF.Functions.ILike(d.Name!, $"%{name}%"))
            .ToListAsync();
    }

    public async Task<IEnumerable<Dish>> GetDishesByRestaurantAsync(Guid restaurantId)
    {
        return await _context.Dishes
            .AsNoTracking()
            .Where(d => d.RestaurantId == restaurantId)
            .ToListAsync();
    }
}

