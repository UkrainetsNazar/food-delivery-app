namespace Payment.API.Interfaces;

using Payment.API.Entities;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task<Payment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId);
    Task UpdateAsync(Payment payment);
}