namespace RestaurantService.Domain.Entities;

public class Dish
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public Guid RestaurantId { get; set; }
    public Restaurant? Restaurant { get; set; }
    public Guid DishCategoryId { get; set; }
    public DishCategory? DishCategory { get; set; }
}
