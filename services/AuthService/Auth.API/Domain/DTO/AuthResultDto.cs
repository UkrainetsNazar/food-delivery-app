namespace Auth.API.Domain.DTO;

public class AuthResultDto
{
    public string AccessToken { get; set; } = default!;
    public string RefreshToken { get; set; } = default!;
}
