using Contracts.Enums;

namespace Contracts.DTO;

public class DishDto
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DishCategory Category { get; set; }
    public string ImageUrl { get; set; } = default!;
    public decimal Price { get; set; }
    public string RestaurantId { get; set; } = default!;
}