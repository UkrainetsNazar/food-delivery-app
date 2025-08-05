using Contracts.Enums;
using Restaurant.API.Domain.Entities;


namespace Restaurant.API.Interfaces;

public interface IRestaurantService
{
    Task<SupportedRestaurant> CreateRestaurantAsync(SupportedRestaurant restaurant);
    Task<SupportedRestaurant?> UpdateRestaurantAsync(SupportedRestaurant restaurant);
    Task<bool> DeleteRestaurantAsync(Guid restaurantId);
    Task<IEnumerable<SupportedRestaurant>> GetAllRestaurantsAsync();
    Task<IEnumerable<DishCategory>> GetRestaurantCategoriesAsync(Guid restaurantId);
}