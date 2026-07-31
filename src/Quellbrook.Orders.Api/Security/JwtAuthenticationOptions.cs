using System.ComponentModel.DataAnnotations;

namespace Quellbrook.Orders.Api.Security;

/// <summary>
/// Where access tokens come from and whom they must be for. Signing keys come from the authority's OpenID Connect
/// metadata, so no key material is configured here.
/// </summary>
public sealed class JwtAuthenticationOptions
{
    public const string SectionName = "Authentication";

    [Required]
    public string Authority { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    public bool HasHttpsAuthority() =>
        Uri.TryCreate(Authority, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps;
}
