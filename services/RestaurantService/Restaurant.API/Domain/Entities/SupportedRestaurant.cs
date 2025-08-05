namespace Restaurant.API.Domain.Entities;

public class SupportedRestaurant
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public ICollection<Dish>? Dishes { get; set; }
}