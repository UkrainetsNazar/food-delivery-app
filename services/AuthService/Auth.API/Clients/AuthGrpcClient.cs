using Auth.API.Interfaces;
using Auth.Grpc;

namespace Auth.API.Clients;

public class AuthGrpcClient(AuthProto.AuthProtoClient client) : IAuthGrpcClient
{
    private readonly AuthProto.AuthProtoClient _client = client;

    public async Task<string> GetUserRoleByIdAsync(Guid userId)
    {
        var response = await _client.GetUserByIdAsync(new GetUserByIdRequest { Id = userId.ToString() });
        return response.Role;
    }
}
