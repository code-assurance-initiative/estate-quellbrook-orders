using Microsoft.AspNetCore.Authorization;

namespace Quellbrook.Orders.Api.Security;

/// <summary>The named policies: each requires one OAuth scope. The gateway is the only client holding them.</summary>
public static class AuthorizationPolicies
{
    public const string ReadOrders = "orders:read";
    public const string WriteOrders = "orders:write";

    public static AuthorizationBuilder AddOrdersPolicies(this AuthorizationBuilder builder) =>
        builder
            .AddPolicy(ReadOrders, policy => policy.RequireScope(ReadOrders))
            .AddPolicy(WriteOrders, policy => policy.RequireScope(WriteOrders));

    private static AuthorizationPolicyBuilder RequireScope(this AuthorizationPolicyBuilder policy, string scope) =>
        policy
            .RequireAuthenticatedUser()
            .RequireAssertion(context => ScopeClaims.HasScope(context.User, scope));
}
