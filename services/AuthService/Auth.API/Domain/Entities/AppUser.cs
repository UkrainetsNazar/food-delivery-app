public class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public byte[]? PasswordHash { get; set; }
    public byte[]? PasswordSalt { get; set; }
    public string Role { get; set; } = "User";
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}