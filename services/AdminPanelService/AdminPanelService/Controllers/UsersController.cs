using AdminPanelService.Clients;
using AdminPanelService.CustomAttribute;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanelService.Controllers;

[ApiController]
[Route("users")]
[RequireAdmin]
public class UsersController(UserManagementClient userClient) : ControllerBase
{
    private readonly UserManagementClient _userClient = userClient;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _userClient.GetUserByIdAsync(id);
        return Ok(user);
    }

    [HttpPost("{id}/block")]
    public async Task<IActionResult> BlockUser(Guid id)
    {
        await _userClient.BlockUserAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/unblock")]
    public async Task<IActionResult> UnblockUser(Guid id)
    {
        await _userClient.UnblockUserAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/role")]
    public async Task<IActionResult> ChangeUserRole(Guid id, [FromBody] string newRole)
    {
        await _userClient.ChangeUserRoleAsync(id, newRole);
        return NoContent();
    }
}
