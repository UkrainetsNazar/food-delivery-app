using Microsoft.EntityFrameworkCore;
using Restaurant.API.Data;
using Restaurant.API.grpcServices;

var builder = WebApplication.CreateBuilder(args);

var useInMemory = builder.Configuration.GetValue<bool>("InMemory");

if (useInMemory)
{
    builder.Services.AddDbContext<RestaurantDbContext>(options =>
        options.UseInMemoryDatabase("RestaurantDb"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<RestaurantDbContext>(options =>
        options.UseNpgsql(connectionString,
        npgOptions => npgOptions.EnableRetryOnFailure()));
}

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaxReceiveMessageSize = 2 * 1024 * 1024;
    options.MaxSendMessageSize = 2 * 1024 * 1024;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Restaurant Service is running!");

app.MapGrpcService<RestaurantService>();
app.MapGrpcService<DishService>();

if (!useInMemory)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
    dbContext.Database.Migrate();
}

app.Run();