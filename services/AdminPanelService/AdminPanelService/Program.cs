var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddHttpContextAccessor();

builder.Services.AddGrpcClient<AuthProto.AuthProtoClient>(o =>
{
    o.Address = new Uri("https://localhost:7289");
});

builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<IRestaurantManagementService, RestaurantManagementService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();