using AdminPanelService.Clients;
using AdminPanelService.CustomAttribute;
using Contracts.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanelService.Controllers;

[ApiController]
[Route("restaurants")]
[RequireAdmin]
public class RestaurantController(RestaurantManagementClient restaurantClient) : ControllerBase
{
    private readonly RestaurantManagementClient _restaurantClient = restaurantClient;

    [HttpPost]
    public async Task<IActionResult> CreateRestaurant([FromBody] RestaurantDto restaurant)
    {
        var createdRestaurant = await _restaurantClient.CreateRestaurantAsync(restaurant);
        return Ok(createdRestaurant);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRestaurant(string id, [FromBody] RestaurantDto restaurant)
    {
        restaurant.Id = id;
        var updated = await _restaurantClient.UpdateRestaurantAsync(restaurant);
        if (updated == null)
            return NotFound($"Restaurant with ID {id} not found");
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRestaurant(string id)
    {
        var success = await _restaurantClient.DeleteRestaurantAsync(id);
        if (!success)
            return NotFound($"Restaurant with ID {id} not found");
        return NoContent();
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAllRestaurants()
    {
        var restaurants = await _restaurantClient.GetAllRestaurantsAsync();
        if (restaurants == null || !restaurants.Any())
            return NotFound();
        return Ok(restaurants);
    }

    [HttpGet("{id}/categories")]
    public async Task<IActionResult> GetRestaurantCategories(string id)
    {
        var categories = await _restaurantClient.GetRestaurantCategoriesAsync(id);
        return Ok(categories);
    }
}
