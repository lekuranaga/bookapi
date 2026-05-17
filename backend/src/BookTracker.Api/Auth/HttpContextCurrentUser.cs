using System.Security.Claims;
using BookTracker.Application.Abstractions;

namespace BookTracker.Api.Auth;

public sealed class HttpContextCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    public Guid Id
    {
        get
        {
            var raw = accessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? accessor.HttpContext?.User?.FindFirstValue("sub");
            return Guid.TryParse(raw, out var id)
                ? id
                : throw new InvalidOperationException("No authenticated user in HttpContext.");
        }
    }
}
