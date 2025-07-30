using Microsoft.EntityFrameworkCore;
using User.API.Data;
using User.API.Services;

var builder = WebApplication.CreateBuilder(args);

var useInMemory = builder.Configuration.GetValue<bool>("InMemory");
builder.Services.AddGrpc();

if (useInMemory)
{
    builder.Services.AddDbContext<UserDbContext>(options =>
        options.UseInMemoryDatabase("AuthDb"));
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