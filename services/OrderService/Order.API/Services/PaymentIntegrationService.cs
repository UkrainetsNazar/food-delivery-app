namespace Order.API.Services;

using Grpc.Net.Client;
using Order.API.Data;
using PaymentGrpcService;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class PaymentIntegrationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly GrpcChannel _channel;
    private readonly PaymentService.PaymentServiceClient _client;
    private readonly IConnection _connection;

    public PaymentIntegrationService(IServiceScopeFactory scopeFactory, IConnection connection)
    {
        _scopeFactory = scopeFactory;
        _connection = connection;

        _channel = GrpcChannel.ForAddress("https://localhost:5005"); // PaymentService
        _client = new PaymentService.PaymentServiceClient(_channel);
    }

    public async Task<string> CreatePaymentForOrder(string orderId, double amount, string currency)
    {
        var response = await _client.CreatePaymentAsync(new CreatePaymentRequest
        {
            OrderId = orderId,
            Amount = amount,
            Currency = currency
        });

        return response.PaymentId;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = _connection.CreateModel();
        channel.ExchangeDeclare("payments", ExchangeType.Fanout);

        var queueName = channel.QueueDeclare().QueueName;
        channel.QueueBind(queueName, "payments", "");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (ch, ea) =>
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());
            var evt = JsonSerializer.Deserialize<PaymentEvent>(json);

            if (evt != null)
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();

                var order = await db.Orders.FindAsync(Guid.Parse(evt.OrderId));
                if (order != null)
                {
                    order.Status = evt.Status == "Succeeded" ? "Paid" : "PaymentFailed";
                    await db.SaveChangesAsync();
                }
            }
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }
}

public record PaymentEvent(string PaymentId, string OrderId, string Status);