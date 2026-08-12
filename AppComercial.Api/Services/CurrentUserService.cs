using System.Security.Claims;
using AppComercial.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AppComercial.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetCurrentUsuario()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return "SISTEMA";

        if (httpContext.Request.Headers.TryGetValue("X-Usuario", out var xUsuario) && !string.IsNullOrWhiteSpace(xUsuario))
        {
            return xUsuario.ToString().Trim();
        }
        if (httpContext.Request.Headers.TryGetValue("X-User-Name", out var xUserName) && !string.IsNullOrWhiteSpace(xUserName))
        {
            return xUserName.ToString().Trim();
        }
        if (httpContext.Request.Headers.TryGetValue("X-User", out var xUser) && !string.IsNullOrWhiteSpace(xUser))
        {
            return xUser.ToString().Trim();
        }

        var identity = httpContext.User?.Identity;
        if (identity == null || !identity.IsAuthenticated)
        {
            return "SISTEMA";
        }

        if (identity.Name == "ApiKeyClient" || string.IsNullOrWhiteSpace(identity.Name))
        {
            return "SISTEMA";
        }

        return identity.Name;
    }
}
