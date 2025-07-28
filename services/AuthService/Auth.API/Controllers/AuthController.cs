using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService, AdminService adminService) : ControllerBase
{
    private readonly AuthService _authService = authService;
    private readonly AdminService _adminService = adminService;

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result == null)
            return BadRequest(new { message = "User already exists" });

        SetRefreshTokenCookie(result.RefreshToken);
        return Ok(new { accessToken = result.AccessToken });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto request)
    {
        var result = await _authService.LoginAsync(request.Email!, request.Password!);
        if (result == null)
            return Unauthorized(new { message = "Invalid email or password" });

        SetRefreshTokenCookie(result.Value.RefreshToken);
        return Ok(new { accessToken = result.Value.AccessToken });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (refreshToken == null)
            return Unauthorized();

        var result = await _authService.RefreshTokenAsync(refreshToken);
        if (result == null)
            return Unauthorized();

        SetRefreshTokenCookie(result.Value.RefreshToken);
        return Ok(new { accessToken = result.Value.AccessToken });
    }

    private void SetRefreshTokenCookie(string token)
    {
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = true,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized("Invalid user ID");

        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrEmpty(refreshToken))
            return BadRequest("No refresh token cookie found.");

        var result = await _authService.RevokeTokenAsync(refreshToken, userId);

        if (!result)
            return NotFound(new { message = "Token not found or already revoked" });

        Response.Cookies.Delete("refreshToken");

        return Ok(new { message = "Logged out successfully" });
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsers();
        return Ok(users);
    }
}
