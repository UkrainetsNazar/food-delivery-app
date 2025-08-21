namespace Payment.API.Services;

using Grpc.Core;
using Payment.API.Entities;
using Payment.API.Interfaces;
using PaymentGrpcService;

public class PaymentService(IPaymentRepository paymentRepository) : PaymentGrpcService.PaymentService.PaymentServiceBase
{
    private readonly IPaymentRepository _paymentRepository = paymentRepository;

    public override async Task<PaymentResponse> CreatePayment(CreatePaymentRequest request, ServerCallContext context)
    {
        var payment = new Payment
        {
            OrderId = Guid.Parse(request.OrderId),
            UserId = Guid.Parse(request.UserId),
            Amount = (decimal)request.Amount,
            Currency = request.Currency,
            PaymentMethod = request.PaymentMethod,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment);

        return MapPayment(payment);
    }

    public override async Task<PaymentResponse> GetPaymentById(GetPaymentByIdRequest request, ServerCallContext context)
    {
        var payment = await _paymentRepository.GetByIdAsync(Guid.Parse(request.PaymentId))
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Payment not found"));

        return MapPayment(payment);
    }

    public override async Task<PaymentsResponse> GetPaymentsByUser(GetPaymentsByUserRequest request, ServerCallContext context)
    {
        var payments = await _paymentRepository.GetByUserIdAsync(Guid.Parse(request.UserId));
        var response = new PaymentsResponse();
        response.Payments.AddRange(payments.Select(MapPayment));
        return response;
    }

    public override async Task<PaymentsResponse> GetPaymentsByOrder(GetPaymentsByOrderRequest request, ServerCallContext context)
    {
        var payments = await _paymentRepository.GetByOrderIdAsync(Guid.Parse(request.OrderId));
        var response = new PaymentsResponse();
        response.Payments.AddRange(payments.Select(MapPayment));
        return response;
    }

    public override async Task<PaymentResponse> UpdatePaymentStatus(UpdatePaymentStatusRequest request, ServerCallContext context)
    {
        var payment = await _paymentRepository.GetByIdAsync(Guid.Parse(request.PaymentId))
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Payment not found"));

        payment.Status = request.Status;
        await _paymentRepository.UpdateAsync(payment);

        return MapPayment(payment);
    }

    private static PaymentResponse MapPayment(Payment payment) =>
        new()
        {
            Id = payment.Id.ToString(),
            OrderId = payment.OrderId.ToString(),
            UserId = payment.UserId.ToString(),
            Amount = (double)payment.Amount,
            Currency = payment.Currency ?? "",
            PaymentMethod = payment.PaymentMethod ?? "",
            Status = payment.Status ?? "",
            CreatedAt = payment.CreatedAt.ToString("O")
        };
}