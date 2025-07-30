namespace Restaurant.API.Domain.Entities;

public class RestaurantCategory
{
    public Guid RestaurantId { get; set; }
    public SupportedRestaurant? Restaurant { get; set; }
    public Guid DishCategoryId { get; set; }
    public DishCategory? DishCategory { get; set; }
    public int SortOrder { get; set; }
}