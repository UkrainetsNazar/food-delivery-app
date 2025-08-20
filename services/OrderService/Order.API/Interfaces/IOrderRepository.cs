namespace Order.API.Interfaces;

public interface IOrderRepository
{
    Task<Entities.Order?> GetByIdAsync(Guid orderId);
    Task<List<Entities.Order>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Entities.Order order);
    Task UpdateAsync(Entities.Order order);
}
