namespace RestaurantService.Domain.Entities;

public class Restaurant
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public ICollection<Dish>? Dishes { get; set; }
    public ICollection<DishCategory>? DishCategories { get; set; }
}
