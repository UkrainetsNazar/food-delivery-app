namespace Auth.API.Interfaces;

public interface IAuthGrpcClient
{
    Task<string> GetUserRoleByIdAsync(Guid userId);
}