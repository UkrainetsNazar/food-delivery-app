using AdminPanelService.Clients;
using AdminPanelService.Mappings;
using Microsoft.OpenApi.Models;
using RestaurantGrpcService;
using UserGrpcService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAutoMapper(typeof(RestaurantManagementProfile));

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaxReceiveMessageSize = 16 * 1024 * 1024;
});

builder.Services.AddGrpcClient<UserService.UserServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcSettings:UserServiceUrl"]!);
})
.ConfigurePrimaryHttpMessageHandler(() => 
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    return handler;
})
.ConfigureChannel(options =>
{
    options.MaxRetryAttempts = 3;
    options.MaxReconnectBackoff = TimeSpan.FromSeconds(5);
});

builder.Services.AddGrpcClient<RestaurantService.RestaurantServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["GrpcSettings:RestaurantServiceUrl"]!);
});

builder.Services.AddScoped<UserManagementClient>();
builder.Services.AddScoped<AuthorizationClient>();
builder.Services.AddScoped<RestaurantManagementClient>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Admin Panel API", Version = "v1" });
    
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "JWT Authorization header",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Identity:Authority"];
        options.Audience = "adminpanel";
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorizationBuilder()
                  .AddPolicy("AdminOnly", policy =>
                  {
                      policy.RequireAuthenticatedUser();
                      policy.RequireClaim("role", "Admin");
                  });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin Panel API v1");
        c.OAuthClientId("adminpanel-swagger");
    });
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.MapControllers();
app.Run();