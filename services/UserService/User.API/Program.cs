using Microsoft.EntityFrameworkCore;
using User.API.Data;
using User.API.Services;

var builder = WebApplication.CreateBuilder(args);

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

app.MapGet("/", () => "User Service is running!");

app.MapGrpcService<UserService>();
app.MapControllers();

if (!useInMemory)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();