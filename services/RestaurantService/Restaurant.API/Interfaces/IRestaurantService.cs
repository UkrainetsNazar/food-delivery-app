using Restaurant.API.Domain.Entities;


namespace Restaurant.API.Interfaces;

public interface IRestaurantService
{
    Task<SupportedRestaurant> CreateRestaurantAsync(SupportedRestaurant restaurant);
    Task<SupportedRestaurant> UpdateRestaurantAsync(SupportedRestaurant restaurant);
    Task<bool> DeleteRestaurantAsync(Guid restaurantId);
    Task<SupportedRestaurant> GetRestaurantByIdAsync(Guid restaurantId);
}