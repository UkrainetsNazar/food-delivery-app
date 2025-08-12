using UserGrpcService;

namespace AdminPanelService.Clients;

public class AuthorizationClient(UserService.UserServiceClient userClient, IHttpContextAccessor accessor)
{
    private readonly UserService.UserServiceClient _userClient = userClient;
    private readonly IHttpContextAccessor _httpContextAccessor = accessor;

    public async Task EnsureAdminAccessAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var response = await _userClient.GetRoleAndStatusAsync(new GetRoleAndStatusRequest
        {
            UserId = userId.ToString()
        });

        if (response.IsBlocked)
            throw new UnauthorizedAccessException("User is blocked.");

        if (response.Role != "Admin")
            throw new UnauthorizedAccessException("Admin privileges required.");
    }
}