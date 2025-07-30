
using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Interfaces;

public interface IDishCategoryService
{
    Task<DishCategory> CreateDishCategoryAsync(DishCategory dishCategory);
    Task<DishCategory> UpdateDishCategoryAsync(DishCategory dishCategory);
    Task<bool> DeleteDishCategoryAsync(Guid dishCategoryId);
    Task<IEnumerable<DishCategory>> GetDishCategoriesByRestaurantAsync(Guid restaurantId);
}