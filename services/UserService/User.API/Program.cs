using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseInMemoryDatabase("UserDb"));

builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<UserRegisteredConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host("rabbitmq", "/", h =>
            {
                h.Username("guest");
                h.Password("guest");
            });

            cfg.ReceiveEndpoint("user-registered-queue", e =>
            {
                e.ConfigureConsumer<UserRegisteredConsumer>(context);
            });
        });
    });

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();