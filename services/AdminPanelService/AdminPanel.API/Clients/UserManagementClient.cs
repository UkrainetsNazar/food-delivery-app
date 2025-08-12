using Contracts.DTO;
using UserGrpcService;

namespace AdminPanelService.Clients;

public class UserManagementClient(UserService.UserServiceClient client)
{
    private readonly UserService.UserServiceClient _client = client;

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        var response = await _client.GetUserByIdAsync(new GetUserByIdRequest { Id = userId.ToString() });
        return new UserDto
        {
            Id = Guid.Parse(response.Id),
            FirstName = response.FirstName,
            LastName = response.LastName,
            Email = response.Email,
            Role = response.Role,
            IsBlocked = response.IsBlocked
        };
    }

    public async Task BlockUserAsync(Guid userId)
    {
        await _client.BlockUserAsync(new BlockUserRequest { Id = userId.ToString() });
    }

    public async Task UnblockUserAsync(Guid userId)
    {
        await _client.UnblockUserAsync(new UnblockUserRequest { Id = userId.ToString() });
    }

    public async Task ChangeUserRoleAsync(Guid userId, string newRole)
    {
        await _client.ChangeUserRoleAsync(new ChangeUserRoleRequest
        {
            Id = userId.ToString(),
            NewRole = newRole
        });
    }

    public async Task<(string role, bool isBlocked)> GetRoleAndStatusAsync(Guid userId)
    {
        var response = await _client.GetRoleAndStatusAsync(new GetRoleAndStatusRequest
        {
            UserId = userId.ToString()
        });

        return (response.Role, response.IsBlocked);
    }
}