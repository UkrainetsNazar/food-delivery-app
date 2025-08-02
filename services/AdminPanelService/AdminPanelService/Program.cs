using AdminPanelService.GrpcClients;
using AdminPanelService.Interfaces;
using AdminPanelService.Services;
using UserService;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddGrpc();

builder.Services.AddGrpcClient<UserGrpc.UserGrpcClient>(o =>
{
    o.Address = new Uri(configuration["GrpcSettings:UserServiceUrl"] ?? "https://localhost:7290");
});

builder.Services.AddScoped<IUserGrpcClient, UserGrpcClient>();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
//builder.Services.AddScoped<IRestaurantManagementService, RestaurantManagementService>();
//builder.Services.AddScoped<IUserManagementService, UserManagementService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRouting();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();
app.Run();
