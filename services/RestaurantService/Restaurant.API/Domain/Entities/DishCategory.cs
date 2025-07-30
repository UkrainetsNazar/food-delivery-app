namespace RestaurantService.Domain.Entities;

public class DishCategory
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public ICollection<Restaurant>? Restaurants { get; set; }
    public ICollection<Dish>? Dishes { get; set; }
}
