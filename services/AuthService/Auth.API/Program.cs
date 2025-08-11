using System.Text;
using Auth.API.Data;
using Auth.API.Domain;
using Auth.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using UserGrpcService;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var useInMemory = builder.Configuration.GetValue<bool>("InMemory");
builder.Services.AddDbContext<AuthDbContext>(options =>
{
    if (useInMemory)
    {
        options.UseInMemoryDatabase("AuthDb");
    }
    else
    {
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            npgOptions => npgOptions
                .EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
                .CommandTimeout(30));
    }
});

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.Interceptors.Add<ExceptionInterceptor>();
    options.MaxReceiveMessageSize = 16 * 1024 * 1024;
});

builder.Services.AddGrpcClient<UserService.UserServiceClient>(o =>
{
    o.Address = new Uri(builder.Configuration["UserServiceUrl"]!);
}).ConfigurePrimaryHttpMessageHandler(() => 
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    return handler;
});

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Auth API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtOptions:Issuer"],
            ValidAudience = builder.Configuration["JwtOptions:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["JwtOptions:Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId("swagger-ui");
        options.OAuthScopes("profile", "openid");
    });
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { Status = "Healthy" }));

app.MapControllers();

if (app.Environment.IsDevelopment() && !useInMemory)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();

public class ExceptionInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {
            return await continuation(request, context);
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }
}