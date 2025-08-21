namespace Payment.API.Services;

using Grpc.Core;
using Payment.API.Interfaces;
using PaymentGrpcService;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

public class PaymentService(IPaymentRepository repository, IConnection connection) 
    : PaymentGrpcService.PaymentService.PaymentServiceBase
{
    private readonly IPaymentRepository _repository = repository;
    private readonly IConnection _connection = connection;

    public override async Task<CreatePaymentResponse> CreatePayment(CreatePaymentRequest request, ServerCallContext context)
    {
        var payment = new Entities.Payment
        {
            Id = Guid.NewGuid(),
            OrderId = Guid.Parse(request.OrderId),
            UserId = Guid.NewGuid(), // можеш додати в gRPC contract UserId
            Amount = (decimal)request.Amount,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(payment);

        _ = Task.Run(async () =>
        {
            await Task.Delay(2000);
            var finalStatus = new Random().Next(2) == 0 ? "Succeeded" : "Failed";
            payment.Status = finalStatus;
            await _repository.UpdateAsync(payment);

            PublishPaymentEvent(payment.Id.ToString(), payment.OrderId.ToString(), finalStatus);
        });

        return new CreatePaymentResponse
        {
            PaymentId = payment.Id.ToString(),
            Status = payment.Status
        };
    }

    public override Task<GetPaymentStatusResponse> GetPaymentStatus(GetPaymentStatusRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetPaymentStatusResponse
        {
            PaymentId = request.PaymentId,
            Status = "Succeeded"
        });
    }

    private void PublishPaymentEvent(string paymentId, string orderId, string status)
    {
        using var channel = _connection.CreateModel();
        channel.ExchangeDeclare("payments", ExchangeType.Fanout);

        var evt = new
        {
            PaymentId = paymentId,
            OrderId = orderId,
            Status = status
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(evt));
        channel.BasicPublish(exchange: "payments", routingKey: "", basicProperties: null, body: body);
    }
}
