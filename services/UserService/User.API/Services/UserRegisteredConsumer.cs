using Contracts.Events;
using MassTransit;

public class UserRegisteredConsumer(IUserService userService) : IConsumer<UserRegisteredEvent>
{
    private readonly IUserService _userService = userService;

    public async Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var message = context.Message;

        var dto = new CreateUserProfileDto
        {
            Id = message.Id,
            FirstName = message.FirstName,
            LastName = message.LastName
        };

        await _userService.CreateProfileAsync(dto);
    }
}