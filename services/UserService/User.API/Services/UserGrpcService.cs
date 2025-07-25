using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using UserService;

public class UserGrpcService(UserDbContext dbContext) : UserGrpc.UserGrpcBase
{
    private readonly UserDbContext _dbContext = dbContext;

    public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var user = new UserProfile
        {
            Id = Guid.Parse(request.UserId),
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        if (await _dbContext.UserProfiles.AnyAsync(u => u.Id == user.Id))
        {
            return new CreateUserResponse { Success = false };
        }

        await _dbContext.UserProfiles.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return new CreateUserResponse { Success = true };
    }
}