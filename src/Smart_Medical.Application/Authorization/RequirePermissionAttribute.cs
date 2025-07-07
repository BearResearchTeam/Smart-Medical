using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace Smart_Medical.Authorization
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequirePermissionAttribute : Attribute, IAsyncActionFilter
    {
        public string PermissionCode { get; }
        public RequirePermissionAttribute(string permissionCode)
        {
            PermissionCode = permissionCode;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var permissionChecker = (IPermissionCheckerService)context.HttpContext.RequestServices.GetService(typeof(IPermissionCheckerService));
            if (permissionChecker == null)
            {
                context.Result = new ForbidResult();
                return;
            }
            // 假设通过HttpContext.User获取用户Id
            var userIdStr = context.HttpContext.User.FindFirst("sub")?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                context.Result = new ForbidResult();
                return;
            }
            var granted = await permissionChecker.IsGrantedAsync(userId, PermissionCode);
            if (!granted)
            {
                context.Result = new ForbidResult();
                return;
            }
            await next();
        }
    }
}
