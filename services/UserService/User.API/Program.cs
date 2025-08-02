using Microsoft.EntityFrameworkCore;
using User.API.Data;
using User.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var useInMemory = builder.Configuration.GetValue<bool>("InMemory");

if (useInMemory)
{
    builder.Services.AddDbContext<UserDbContext>(options =>
        options.UseInMemoryDatabase("UserDb"));
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<UserDbContext>(options =>
        options.UseNpgsql(connectionString));
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGrpcService<UserGrpcService>();
app.MapControllers();

app.Run();
