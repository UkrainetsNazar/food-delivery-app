using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Order.API.Interfaces;
using Order.API.Repositories;
using Order.API.Services;
using RestaurantGrpcService;
using RabbitMQ.Client;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrderDb"));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddGrpc();

builder.Services.AddGrpcClient<DishService.DishServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["RestaurantServiceUrl"]!);
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

var app = builder.Build();

app.MapGrpcService<OrderService>();
app.MapGet("/", () => "Order Service is running...");

app.Run();
