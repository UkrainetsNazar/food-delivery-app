using Contracts.DTO;

namespace AdminPanelService.Interfaces;

public interface IUserGrpcClient
{
    Task<UserDto> GetUserByIdAsync(Guid userId);
    Task BlockUserAsync(Guid userId);
    Task ChangeUserRoleAsync(Guid userId, string newRole);
    Task UnblockUserAsync(Guid userId);
    Task<(string role, bool isBlocked)> GetRoleAndStatus(Guid userId);
}