namespace Auth.API.Domain.Entities;

public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}