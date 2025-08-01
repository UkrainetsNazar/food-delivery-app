namespace AdminPanelService.Interfaces;

public interface IUserManagementService
{
    Task<List<GetUserDto>> GetAllUsers();
    Task<GetUserDto> GetUserById(Guid userId);
    Task UpdateUserRole(Guid userId, string newRole);
    Task DeleteUser(Guid userId);
}