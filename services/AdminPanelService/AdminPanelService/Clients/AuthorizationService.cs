using AdminPanelService.Interfaces;

namespace AdminPanelService.Clients;

public class AuthorizationClient(IUserGrpcClient userClient, IHttpContextAccessor accessor) : IAuthorizationClient
{
    private readonly IUserGrpcClient _userClient = userClient;
    private readonly IHttpContextAccessor _httpContextAccessor = accessor;

    public async Task EnsureAdminAccessAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var (role, isBlocked) = await _userClient.GetRoleAndStatus(Guid.Parse(userId));

        if (isBlocked)
            throw new UnauthorizedAccessException("User is blocked.");

        if (role != "Admin")
            throw new UnauthorizedAccessException("Admin privileges required.");
    }
}