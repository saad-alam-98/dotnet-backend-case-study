using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class AuthPermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
{
    private readonly string[] _requiredPermissions;

    public AuthPermissionAttribute(params string[] requiredPermissions)
    {
        _requiredPermissions = requiredPermissions ?? Array.Empty<string>();
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult(); // 401
            return;
        }

        // If no specific permissions required, just ensure authentication
        if (_requiredPermissions.Length == 0)
            return;

        var userPermissions = user.Claims
            .Where(c => string.Equals(c.Type, "permission", StringComparison.OrdinalIgnoreCase))
            .Select(c => c.Value)
            .ToList();

        var hasAtLeastOne = _requiredPermissions.Any(r =>
            userPermissions.Any(up => up == r));

        if (!hasAtLeastOne)
        {
            context.Result = new ForbidResult(); // 403
        }
    }
}
