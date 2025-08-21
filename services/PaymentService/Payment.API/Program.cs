using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Interfaces;
using Payment.API.Repositories;
using Payment.API.Services;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseInMemoryDatabase("PaymentDb"));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddSingleton<PaymentService>();
builder.Services.AddSingleton<PaymentConsumer>();


builder.Services.AddGrpc();

builder.Services.AddGrpcClient<OrderGrpcService.OrderService.OrderServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcSettings:OrderUrl"]!);
});

builder.Services.AddSingleton<IConnectionFactory>(sp =>
    new ConnectionFactory()
    {
        HostName = builder.Configuration["RabbitMQ:Host"] ?? "localhost",
        UserName = builder.Configuration["RabbitMQ:User"] ?? "guest",
        Password = builder.Configuration["RabbitMQ:Pass"] ?? "guest"
    });

builder.Services.AddSingleton<IConnection>(sp =>
{
    var factory = sp.GetRequiredService<IConnectionFactory>();
    return factory.CreateConnection();
});

builder.Services.AddSingleton<IModel>(sp =>
{
    var connection = sp.GetRequiredService<IConnection>();
    var channel = connection.CreateModel();
    channel.QueueDeclare("order_created", durable: true, exclusive: false, autoDelete: false);
    return channel;
});

builder.Services.AddHostedService<PaymentConsumer>();

var app = builder.Build();

app.MapGrpcService<PaymentService>();

app.MapGet("/", () => "Payment Service is running");

app.Run();
