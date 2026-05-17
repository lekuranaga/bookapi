using System.Security.Claims;
using BookTracker.Application.Abstractions;

namespace BookTracker.Api.Auth;

public sealed class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid Id
    {
        get
        {
            var raw = _accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? _accessor.HttpContext?.User?.FindFirstValue("sub");
            return Guid.TryParse(raw, out var id)
                ? id
                : throw new InvalidOperationException("No authenticated user in HttpContext.");
        }
    }
}
