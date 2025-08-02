using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using User.API.Data;
using User.API.Domain.Entities;
using UserService;

namespace User.API.Services;

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

    public override async Task<GetRoleAndStatusResponse> GetRoleAndStatus(GetRoleAndStatusRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid UserId format"));
        }

        var user = await _dbContext.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return new GetRoleAndStatusResponse
        {
            Role = user.Role,
            IsBlocked = user.IsBlocked
        };
    }


    public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID"));

        var user = await _dbContext.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return new GetUserByIdResponse
        {
            Id = user.Id.ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? "",
            Role = user.Role,
            IsBlocked = user.IsBlocked
        };
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> BlockUser(BlockUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID"));

        var user = await _dbContext.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        user.IsBlocked = true;
        await _dbContext.SaveChangesAsync();

        return new Google.Protobuf.WellKnownTypes.Empty();
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> UnblockUser(UnblockUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID"));

        var user = await _dbContext.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        user.IsBlocked = false;
        await _dbContext.SaveChangesAsync();

        return new Google.Protobuf.WellKnownTypes.Empty();
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> ChangeUserRole(ChangeUserRoleRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID"));

        var user = await _dbContext.UserProfiles.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        user.Role = request.NewRole;
        await _dbContext.SaveChangesAsync();

        return new Google.Protobuf.WellKnownTypes.Empty();
    }

}