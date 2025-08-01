using Restaurant.API.Domain.Entities;

namespace Restaurant.API.Interfaces;

public interface IDishService
{
    Task<Dish> CreateDishAsync(Dish dish);
    Task<Dish?> UpdateDishAsync(Dish dish);
    Task<bool> DeleteDishAsync(Guid dishId);
    Task<IEnumerable<Dish>> GetDishesByRestaurantAndCategoryAsync(Guid restaurantId, Guid categoryId);
    Task<IEnumerable<Dish>> GetDishesByNameAsync(string name);
}