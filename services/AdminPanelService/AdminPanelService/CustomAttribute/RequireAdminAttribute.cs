namespace AdminPanelService.CustomAttribute;

public class RequireAdminAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var authorizationService = context.HttpContext.RequestServices.GetRequiredService<IAuthorizationService>();
        try
        {
            await authorizationService.EnsureAdminAccessAsync();
            await next();
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Result = new ForbidResult();
        }
    }
}
