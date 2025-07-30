namespace Restaurant.API.Domain.Entities;

public class DishCategory
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public ICollection<RestaurantCategory>? RestaurantCategories { get; set; }
    public ICollection<Dish>? Dishes { get; set; }
}
