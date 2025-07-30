using Microsoft.EntityFrameworkCore;
using RestaurantService.Data;

var builder = WebApplication.CreateBuilder(args);

var useInMemory = builder.Configuration.GetValue<bool>("InMemory");

if (useInMemory)
{
    builder.Services.AddDbContext<RestaurantDbContext>(options =>
        options.UseInMemoryDatabase("AuthDb"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<RestaurantDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();

