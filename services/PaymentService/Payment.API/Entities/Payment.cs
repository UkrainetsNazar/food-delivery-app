namespace Payment.API.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}