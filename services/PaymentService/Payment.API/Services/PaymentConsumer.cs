namespace Payment.API.Services;

using System.Text;
using System.Text.Json;
using Payment.API.Entities;
using Payment.API.Interfaces;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using Contracts.Events;

public class PaymentConsumer(IModel channel, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly IModel _channel = channel;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var order = JsonSerializer.Deserialize<OrderCreatedEvent>(message);

            if (order != null)
            {
                Console.WriteLine($"[PaymentService] Processing payment for Order {order.OrderId} with total {order.TotalAmount}");

                using var scope = _scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    UserId = order.UserId,
                    Amount = order.TotalAmount,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                await repo.AddAsync(payment);

                // Можна відразу викликати gRPC PaymentService для емуляції процесингу
                // або чекати PublishPaymentEvent з PaymentService
            }
        };

        _channel.BasicConsume(queue: "order_created", autoAck: true, consumer: consumer);

        return Task.CompletedTask;
    }
}
