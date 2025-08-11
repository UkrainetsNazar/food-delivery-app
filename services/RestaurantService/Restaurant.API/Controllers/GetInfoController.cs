using Microsoft.AspNetCore.Mvc;
using Restaurant.API.Domain.Services;

namespace Restaurant.API.Controllers;

[ApiController]
[Route("restaurants")]
public class GetInfoController(RestaurantDomainService restaurantDomainService, DishDomainService dishDomainService) : ControllerBase
{
    private readonly RestaurantDomainService _restaurantDomainService = restaurantDomainService;
    private readonly DishDomainService _dishDomainService = dishDomainService;

    [HttpGet("restaurants")]
    public async Task<IActionResult> GetAllRestaurants()
    {
        var restaurants = await _restaurantDomainService.GetAllRestaurantsAsync();
        return Ok(restaurants);
    }

    [HttpGet("restaurants/{restaurantId}/dishes")]
    public async Task<IActionResult> GetDishesByRestaurant(Guid restaurantId)
    {
        var dishes = await _dishDomainService.GetDishesByRestaurantAsync(restaurantId);
        return Ok(dishes);
    }

    [HttpGet("dishes/{dishId}")]
    public async Task<IActionResult> GetDishById(Guid dishId)
    {
        var dish = await _dishDomainService.GetDishByIdAsync(dishId);
        if (dish == null)
            return NotFound();

        return Ok(dish);
    }

    [HttpGet("dishes/search")]
    public async Task<IActionResult> SearchDishes([FromQuery] string name)
    {
        var dishes = await _dishDomainService.GetDishesByNameAsync(name);
        return Ok(dishes);
    }

    [HttpGet("restaurants/{restaurantId}/categories")]
    public async Task<IActionResult> GetRestaurantCategories(Guid restaurantId)
    {
        var categories = await _restaurantDomainService.GetRestaurantCategoriesAsync(restaurantId);
        return Ok(categories);
    }
}
