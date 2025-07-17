using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpPost]
    public async Task<IActionResult> CreateProfile([FromBody] CreateUserProfileDto dto)
    {
        var created = await _userService.CreateProfileAsync(dto);
        if (!created)
            return Conflict("User profile already exists");

        System.Console.WriteLine("CreateProfile is working");

        return Ok();
    }
}