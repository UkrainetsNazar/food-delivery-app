using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using User.API.Data;
using User.API.Domain.Entities;
using UserGrpcService;

namespace User.API.Services;

public class UserService(UserDbContext dbContext) : UserGrpcService.UserService.UserServiceBase
{
    private readonly UserDbContext _dbContext = dbContext;

    public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.UserId, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid UserId format"));
        }

        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "First name and last name are required"));
        }

        if (await _dbContext.UserProfiles.AnyAsync(u => u.Id == userId))
        {
            return new CreateUserResponse { Success = false };
        }

        var user = new UserProfile
        {
            Id = userId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim()
        };

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

        var user = await _dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId)
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
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID"));
        }

        var user = await _dbContext.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return new GetUserByIdResponse
        {
            Id = user.Id.ToString(),
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Role = user.Role,
            IsBlocked = user.IsBlocked
        };
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> BlockUser(BlockUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID"));
        }

        var user = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        if (user.IsBlocked)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "User is already blocked"));
        }

        user.IsBlocked = true;
        await _dbContext.SaveChangesAsync();

        return new Google.Protobuf.WellKnownTypes.Empty();
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> UnblockUser(UnblockUserRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID"));
        }

        var user = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        if (!user.IsBlocked)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "User is not blocked"));
        }

        user.IsBlocked = false;
        await _dbContext.SaveChangesAsync();

        return new Google.Protobuf.WellKnownTypes.Empty();
    }

    public override async Task<Google.Protobuf.WellKnownTypes.Empty> ChangeUserRole(ChangeUserRoleRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var userId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID"));
        }

        if (string.IsNullOrWhiteSpace(request.NewRole))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Role cannot be empty"));
        }

        var user = await _dbContext.UserProfiles
            .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        if (user.Role == request.NewRole)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "User already has this role"));
        }

        user.Role = request.NewRole;
        await _dbContext.SaveChangesAsync();

        return new Google.Protobuf.WellKnownTypes.Empty();
    }
}