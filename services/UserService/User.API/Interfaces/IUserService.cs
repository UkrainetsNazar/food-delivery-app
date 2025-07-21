public interface IUserService
{
    Task<bool> CreateProfileAsync(CreateUserProfileDto dto);
}
