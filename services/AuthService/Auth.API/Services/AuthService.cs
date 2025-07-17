using System.Security.Cryptography;
using System.Text;
using Contracts.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;

public class AuthService(AuthDbContext context, JwtService jwtService, IPublishEndpoint publishEndpoint)
{
    private readonly AuthDbContext _context = context;
    private readonly JwtService _jwtService = jwtService;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task<(string AccessToken, string RefreshToken)?> LoginAsync(string email, string password)
    {
        var user = await _context.AppUsers.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return null;

        if (!VerifyPassword(password, user.PasswordHash!, user.PasswordSalt!))
            return null;

        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, user.Role);
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

        user.RefreshTokens.Where(t => t.IsActive).ToList().ForEach(t => t.Revoked = DateTime.UtcNow);

        var newRefreshToken = GenerateRefreshToken();
        refreshToken.Revoked = DateTime.UtcNow;
        user.RefreshTokens.Add(newRefreshToken);

        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, user.Role);
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

    public async Task<(string, string)?> RegisterAsync(string email, string password, string firstName, string lastName)
    {
        if (await _context.AppUsers.AnyAsync(u => u.Email == email))
            return null;

        CreatePasswordHash(password, out byte[] hash, out byte[] salt);

        var user = new AppUser
        {
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            RefreshTokens = new List<RefreshToken>()
        };

        var refreshToken = GenerateRefreshToken();
        user.RefreshTokens.Add(refreshToken);

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new UserRegisteredEvent
        {
            Id = user.Id,
            FirstName = firstName,
            LastName = lastName
        });

        var accessToken = _jwtService.GenerateToken(user.Id, user.Email, user.Role);
        return (accessToken, refreshToken.Token);
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
