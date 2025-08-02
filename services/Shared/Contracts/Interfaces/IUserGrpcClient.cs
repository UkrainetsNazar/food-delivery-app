using Contracts.DTO;

namespace Contracts.Interfaces;

public interface IUserGrpcClient
{
    Task BlockUser(Guid userId);
    Task ChangeUserRole(Guid userId, string newRole);
    Task<UserDto> GetUserById(Guid userId);
    Task UnblockUser(Guid userId);
    Task<(string Role, bool IsBlocked)> GetRoleAndStatus(Guid userId);
}
