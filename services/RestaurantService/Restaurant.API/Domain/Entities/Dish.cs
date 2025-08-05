using Contracts.Enums;

namespace Restaurant.API.Domain.Entities;

public class Dish
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DishCategory Category { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public Guid RestaurantId { get; set; }
    public SupportedRestaurant? Restaurant { get; set; }
}
