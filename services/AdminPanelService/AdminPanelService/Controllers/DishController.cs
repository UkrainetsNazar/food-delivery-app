using AdminPanelService.Clients;
using AdminPanelService.CustomAttribute;
using Contracts.DTO;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanelService.Controllers;

[ApiController]
[Route("admin/dishes")]
[RequireAdmin]
public class DishController(RestaurantManagementClient restaurantClient) : ControllerBase
{
    private readonly RestaurantManagementClient _restaurantClient = restaurantClient;

    [HttpPost("{restaurantId}/dishes")]
    public async Task<IActionResult> CreateDish(string restaurantId, [FromBody] DishDto dish)
    {
        dish.RestaurantId = restaurantId;
        var createdDish = await _restaurantClient.CreateDishAsync(dish);
        return Ok(createdDish);
    }

    [HttpPut("dishes/{id}")]
    public async Task<IActionResult> UpdateDish(string id, [FromBody] DishDto dish)
    {
        dish.Id = id;
        var updated = await _restaurantClient.UpdateDishAsync(dish);
        if (updated == null)
            return NotFound($"Dish with ID {id} not found");
        return Ok(updated);
    }

    [HttpDelete("dishes/{id}")]
    public async Task<IActionResult> DeleteDish(string id)
    {
        var success = await _restaurantClient.DeleteDishAsync(id);
        if (!success)
            return NotFound($"Dish with ID {id} not found");
        return NoContent();
    }

    [HttpGet("dishes/{id}")]
    public async Task<IActionResult> GetDishById(string id)
    {
        var dish = await _restaurantClient.GetDishByIdAsync(id);
        if (dish == null)
            return NotFound();
        return Ok(dish);
    }

    [HttpGet("dishes/search")]
    public async Task<IActionResult> GetDishesByName([FromQuery] string name)
    {
        var dishes = await _restaurantClient.GetDishesByNameAsync(name);
        return Ok(dishes);
    }

    [HttpGet("{restaurantId}/dishes")]
    public async Task<IActionResult> GetDishesByRestaurant(string restaurantId)
    {
        var dishes = await _restaurantClient.GetDishesByRestaurantAsync(restaurantId);
        return Ok(dishes);
    }
}