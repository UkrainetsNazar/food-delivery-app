using AdminPanelService.Clients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AdminPanelService.CustomAttribute;

public class RequireAdminAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var authorizationService = context.HttpContext.RequestServices.GetRequiredService<AuthorizationClient>();
        try
        {
            await authorizationService.EnsureAdminAccessAsync();
            await next();
        }
        catch (UnauthorizedAccessException)
        {
            context.Result = new ForbidResult();
        }
    }
}
