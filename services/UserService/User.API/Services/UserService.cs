using Microsoft.EntityFrameworkCore;

public class UserService(UserDbContext dbContext) : IUserService
{
    private readonly UserDbContext _dbContext = dbContext;

    public async Task<bool> CreateProfileAsync(CreateUserProfileDto dto)
    {
        if (await _dbContext.UserProfiles.AnyAsync(x => x.Id == dto.Id))
            return false;

        var user = new UserProfile
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName
        };

        _dbContext.UserProfiles.Add(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }
}