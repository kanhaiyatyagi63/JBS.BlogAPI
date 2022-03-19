using JBS.DataLayer.Abstracts;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace JBS.Service;
public class UserContextService : IUserContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public UserContextService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetUserId()
    {
        if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.User == null)
            return null;

        var claim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        if (claim == null)
            return null;

        return claim.Value;
    }

    public string? GetUserRole()
    {
        if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.User == null)
            return null;

        var claim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
        if (claim == null)
            return null;

        return claim.Value;
    }

    public string GetIpAddress()
    {
        return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }
}
