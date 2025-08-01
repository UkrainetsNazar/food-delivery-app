using Microsoft.EntityFrameworkCore;
using Restaurant.API.Data;
using Restaurant.API.Domain.Entities;
using Restaurant.API.Interfaces;

namespace Restaurant.API.Services;

public class DishCategoryService(RestaurantDbContext context) : IDishCategoryService
{
    private readonly RestaurantDbContext _context = context;

    public async Task<DishCategory> CreateDishCategoryAsync(DishCategory dishCategory)
    {
        _context.DishCategories.Add(dishCategory);
        await _context.SaveChangesAsync();

        return dishCategory;
    }

    public async Task<DishCategory?> UpdateDishCategoryAsync(DishCategory category)
    {
        var existingDishCategory = await _context.DishCategories.FindAsync(category.Id);
        if (existingDishCategory is null) return null;

        _context.Entry(existingDishCategory).CurrentValues.SetValues(category);
        await _context.SaveChangesAsync();

        return existingDishCategory;
    }

    public async Task<bool> DeleteDishCategoryAsync(Guid dishCategoryId)
    {
        var dishCategory = await _context.DishCategories.FindAsync(dishCategoryId);
        if (dishCategory is null) return false;

        _context.DishCategories.Remove(dishCategory);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<DishCategory>> GetDishCategoriesByRestaurantAsync(Guid restaurantId)
    {
        var categories = await _context.RestaurantCategories
        .AsNoTracking()
        .Where(rc => rc.RestaurantId == restaurantId)
        .OrderBy(rc => rc.SortOrder)
        .Select(rc => rc.DishCategory)
        .Where(dc => dc != null)
        .ToListAsync();

        return categories!;
    }
}