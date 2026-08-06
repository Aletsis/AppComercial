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
        var identity = _httpContextAccessor.HttpContext?.User?.Identity;
        if (identity == null || !identity.IsAuthenticated)
        {
            return "SISTEMA";
        }

        return identity.Name ?? "SISTEMA";
    }
}
