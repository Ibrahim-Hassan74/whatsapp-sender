using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WhatsAppServices.API.Filter
{
    public class CustomAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var token = context.HttpContext.Request.Headers["x-node-token"].FirstOrDefault();

            var user = context.HttpContext.User;

            if ((user?.Identity != null && user.Identity.IsAuthenticated) || (token == config["Node:Token"]))
            {
                await next();
            }

            context.Result = new UnauthorizedResult();
            return;
        }
    }
}
