using Auth.API.Data;
using Auth.Grpc;
using Grpc.Core;

namespace Auth.API.Services;

public class AuthGrpcService(AuthDbContext context) : AuthProto.AuthProtoBase
{
    private readonly AuthDbContext _context = context;
    public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        var userId = Guid.Parse(request.Id);
        var user = await _context.AppUsers.FindAsync(userId);
        if (user is null)
            throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return new GetUserByIdResponse
        {
            Id = user.Id.ToString(),
            Email = user.Email,
            Role = user.Role
        };
    }
}
