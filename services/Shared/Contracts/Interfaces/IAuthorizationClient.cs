namespace AdminPanelService.Interfaces;

public interface IAuthorizationClient
{
    Task EnsureAdminAccessAsync();
}
