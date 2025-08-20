using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Order.API.Interfaces;
using Order.API.Repositories;
using Order.API.Services;
using RestaurantGrpcService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrderDb"));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();

builder.Services.AddGrpc();

builder.Services.AddGrpcClient<DishService.DishServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["RestaurantServiceUrl"]!);
});

var app = builder.Build();

app.MapGrpcService<OrderService>();
app.MapGet("/", () => "Order Service is running...");

app.Run();
