using Microsoft.EntityFrameworkCore;

public class AdminService(AuthDbContext dbContext)
{
    private readonly AuthDbContext _dbContext = dbContext;

    public async Task<List<GetUserDto>> GetAllUsers()
    {
        var users = await _dbContext.AppUsers
            .Select(user => new GetUserDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                RefreshTokens = user.RefreshTokens
            })
            .ToListAsync();

        return users;
    }
}