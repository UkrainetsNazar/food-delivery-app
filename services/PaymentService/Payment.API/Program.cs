using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Interfaces;
using Payment.API.Repositories;
using Payment.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseInMemoryDatabase("PaymentDb"));

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

builder.Services.AddGrpc();
builder.Services.AddGrpcClient<OrderGrpcService.OrderService.OrderServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["OrderServiceUrl"]!);
});

var app = builder.Build();

app.MapGrpcService<PaymentService>();

app.MapGet("/", () => "Payment Service is running");

app.Run();
