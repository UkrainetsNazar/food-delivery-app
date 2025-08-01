namespace AdminPanelService.Services;

public class AuthorizationService(IAuthGrpcClient authClient, IHttpContextAccessor accessor) : IAuthorizationService
{
    private readonly IAuthGrpcClient _authClient = authClient;
    private readonly IHttpContextAccessor _httpContextAccessor = accessor;

    public async Task EnsureAdminAccessAsync()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("User not authenticated.");

        var role = await _authClient.GetUserRoleByIdAsync(Guid.Parse(userId));
        if (role != "Admin")
            throw new UnauthorizedAccessException("Admin privileges required.");
    }
}
