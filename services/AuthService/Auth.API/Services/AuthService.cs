using System.Security.Cryptography;
using System.Text;
using Auth.API.Data;
using Auth.API.Domain;
using Auth.API.Domain.DTO;
using Auth.API.Domain.Entities;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using UserService;

namespace Auth.API.Services;

public class AuthService(AuthDbContext context, JwtService jwtService, UserGrpc.UserGrpcClient userClient)
{
    private readonly AuthDbContext _context = context;
    private readonly JwtService _jwtService = jwtService;
    private readonly UserGrpc.UserGrpcClient _userClient = userClient;

    public async Task<(string AccessToken, string RefreshToken)?> LoginAsync(string email, string password)
    {
        var user = await _context.AppUsers.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !VerifyPassword(password, user.PasswordHash!, user.PasswordSalt!))
            return null;

        var grpcResponse = await _userClient.GetRoleAndStatusAsync(new GetRoleAndStatusRequest
        {
            UserId = user.Id.ToString()
        });

        if (grpcResponse.IsBlocked)
            return null;

        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, grpcResponse.Role);
        var refreshToken = GenerateRefreshToken();

        user.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return (accessToken, refreshToken.Token);
    }

    public async Task<(string AccessToken, string RefreshToken)?> RefreshTokenAsync(string token)
    {
        var refreshToken = await _context.RefreshTokens
            .Include(r => r.AppUser)
            .FirstOrDefaultAsync(r => r.Token == token);

        if (refreshToken == null || !refreshToken.IsActive)
            return null;

        var user = refreshToken.AppUser;

        var response = await _userClient.GetRoleAndStatusAsync(new GetRoleAndStatusRequest
        {
            UserId = user.Id.ToString()
        });

        if (response.IsBlocked)
            return null;

        user.RefreshTokens.Where(t => t.IsActive).ToList().ForEach(t => t.Revoked = DateTime.UtcNow);

        var newRefreshToken = GenerateRefreshToken();
        refreshToken.Revoked = DateTime.UtcNow;
        user.RefreshTokens.Add(newRefreshToken);

        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, response.Role);
        await _context.SaveChangesAsync();

        return (accessToken, newRefreshToken.Token);
    }

    public async Task<bool> RevokeTokenAsync(string token, Guid userId)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == token && r.AppUserId == userId);

        if (refreshToken == null || !refreshToken.IsActive)
            return false;

        refreshToken.Revoked = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AuthResultDto?> RegisterAsync(RegisterDto registerDto)
    {
        if (await _context.AppUsers.AnyAsync(u => u.Email == registerDto.Email))
            return null;

        CreatePasswordHash(registerDto.Password!, out byte[] hash, out byte[] salt);

        var user = new AppUser
        {
            Email = registerDto.Email!,
            PasswordHash = hash,
            PasswordSalt = salt,
            RefreshTokens = new List<RefreshToken>()
        };

        var refreshToken = GenerateRefreshToken();
        user.RefreshTokens.Add(refreshToken);

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();

        var grpcRequest = new CreateUserRequest
        {
            UserId = user.Id.ToString(),
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName
        };

        try
        {
            var grpcResponse = await _userClient.CreateUserAsync(grpcRequest);
            if (!grpcResponse.Success)
                Console.WriteLine("UserService failed to create user");
        }
        catch (RpcException ex)
        {
            Console.WriteLine($"gRPC error while creating user profile: {ex.Status.Detail}");
        }

        var roleResponse = await _userClient.GetRoleAndStatusAsync(new GetRoleAndStatusRequest
        {
            UserId = user.Id.ToString()
        });

        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, roleResponse.Role);
        return new AuthResultDto { AccessToken = accessToken, RefreshToken = refreshToken.Token };
    }


    private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
    {
        using var hmac = new HMACSHA256();
        salt = hmac.Key;
        hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
    {
        using var hmac = new HMACSHA256(storedSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
    }

    private RefreshToken GenerateRefreshToken()
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
            Expires = DateTime.UtcNow.AddDays(7),
            Created = DateTime.UtcNow
        };
    }
}
