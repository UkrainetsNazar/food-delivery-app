using System.Security.Cryptography;
using System.Text;
using Auth.API.Data;
using Auth.API.Domain.DTO;
using Auth.API.Domain.Entities;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using UserGrpcService;

namespace Auth.API.Services;

public class AuthService(
    AuthDbContext context,
    JwtService jwtService,
    UserService.UserServiceClient userClient,
    ILogger<AuthService> logger)
{
    private readonly AuthDbContext _context = context;
    private readonly JwtService _jwtService = jwtService;
    private readonly UserService.UserServiceClient _userClient = userClient;
    private readonly ILogger<AuthService> _logger = logger;

    public async Task<(string AccessToken, string RefreshToken)?> LoginAsync(string email, string password)
    {
        try
        {
            var user = await _context.AppUsers
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == email);
            
            if (user == null || !VerifyPassword(password, user.PasswordHash!, user.PasswordSalt!))
            {
                _logger.LogWarning("Login failed for email: {Email}", email);
                return null;
            }

            var grpcResponse = await _userClient.GetRoleAndStatusAsync(
                new GetRoleAndStatusRequest { UserId = user.Id.ToString() },
                deadline: DateTime.UtcNow.AddSeconds(5));

            if (grpcResponse.IsBlocked)
            {
                _logger.LogWarning("Login attempt for blocked user: {UserId}", user.Id);
                return null;
            }

            var accessToken = _jwtService.GenerateToken(user.Id, user.Email, grpcResponse.Role);
            var refreshToken = GenerateRefreshToken();

            user.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return (accessToken, refreshToken.Token);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.DeadlineExceeded)
        {
            _logger.LogError(ex, "Timeout while checking user status for email: {Email}", email);
            throw new ApplicationException("Service unavailable");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for email: {Email}", email);
            throw;
        }
    }

    public async Task<(string AccessToken, string RefreshToken)?> RefreshTokenAsync(string token)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var refreshToken = await _context.RefreshTokens
                .Include(r => r.AppUser)
                .FirstOrDefaultAsync(r => r.Token == token);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                _logger.LogWarning("Invalid refresh token attempt");
                return null;
            }

            var user = refreshToken.AppUser;

            var response = await _userClient.GetRoleAndStatusAsync(
                new GetRoleAndStatusRequest { UserId = user.Id.ToString() },
                deadline: DateTime.UtcNow.AddSeconds(5));

            if (response.IsBlocked)
            {
                _logger.LogWarning("Refresh token attempt for blocked user: {UserId}", user.Id);
                return null;
            }

            var activeTokens = await _context.RefreshTokens
                .Where(t => t.AppUserId == user.Id && t.IsActive)
                .ToListAsync();

            foreach (var activeToken in activeTokens)
            {
                activeToken.Revoked = DateTime.UtcNow;
            }

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);

            var accessToken = _jwtService.GenerateToken(user.Id, user.Email, response.Role);
            
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (accessToken, newRefreshToken.Token);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during token refresh");
            throw;
        }
    }

    public async Task<bool> RevokeTokenAsync(string token, Guid userId)
    {
        try
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token && r.AppUserId == userId);

            if (refreshToken == null || !refreshToken.IsActive)
                return false;

            refreshToken.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<AuthResultDto?> RegisterAsync(RegisterDto registerDto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            if (await _context.AppUsers.AnyAsync(u => u.Email == registerDto.Email))
            {
                _logger.LogWarning("Registration attempt with existing email: {Email}", registerDto.Email);
                return null;
            }

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

            try
            {
                var grpcResponse = await _userClient.CreateUserAsync(
                    new CreateUserRequest
                    {
                        UserId = user.Id.ToString(),
                        FirstName = registerDto.FirstName,
                        LastName = registerDto.LastName,
                        Email = registerDto.Email
                    },
                    deadline: DateTime.UtcNow.AddSeconds(5));

                if (!grpcResponse.Success)
                {
                    _logger.LogError("Failed to create user profile in UserService for: {Email}", registerDto.Email);
                }
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "gRPC error while creating user profile for: {Email}", registerDto.Email);
                await transaction.RollbackAsync();
                throw new ApplicationException("Failed to create user profile");
            }

            var roleResponse = await _userClient.GetRoleAndStatusAsync(
                new GetRoleAndStatusRequest { UserId = user.Id.ToString() },
                deadline: DateTime.UtcNow.AddSeconds(5));

            var accessToken = _jwtService.GenerateToken(user.Id, user.Email, roleResponse.Role);
            
            await transaction.CommitAsync();
            
            return new AuthResultDto 
            { 
                AccessToken = accessToken, 
                RefreshToken = refreshToken.Token 
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during registration for email: {Email}", registerDto.Email);
            throw;
        }
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