using System.Security.Claims;

namespace Quellbrook.Orders.Api.Security;

public static class ScopeClaims
{
    /// <summary>The OAuth 2.0 access-token claim (RFC 8693 §4.2): one space-delimited string of scopes.</summary>
    public const string ScopeClaimType = "scope";

    public static bool HasScope(ClaimsPrincipal user, string scope)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user
            .FindAll(ScopeClaimType)
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Contains(scope, StringComparer.Ordinal);
    }
}
