namespace Payment.API.Repositories;

using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Entities;
using Payment.API.Interfaces;

public class PaymentRepository(PaymentDbContext context) : IPaymentRepository
{
    private readonly PaymentDbContext _context = context;

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
        => await _context.Payments.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Payment>> GetByUserIdAsync(Guid userId)
        => await _context.Payments.Where(p => p.UserId == userId).ToListAsync();

    public async Task<IEnumerable<Payment>> GetByOrderIdAsync(Guid orderId)
        => await _context.Payments.Where(p => p.OrderId == orderId).ToListAsync();

    public async Task UpdateAsync(Payment payment)
    {
        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();
    }
}
