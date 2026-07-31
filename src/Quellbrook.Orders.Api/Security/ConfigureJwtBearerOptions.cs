using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace Quellbrook.Orders.Api.Security;

/// <summary>Applies <see cref="JwtAuthenticationOptions"/> to the bearer handler.</summary>
public sealed class ConfigureJwtBearerOptions(IOptions<JwtAuthenticationOptions> settings) : IConfigureNamedOptions<JwtBearerOptions>
{
    public void Configure(string? name, JwtBearerOptions options)
    {
        if (name == JwtBearerDefaults.AuthenticationScheme)
        {
            Configure(options);
        }
    }

    public void Configure(JwtBearerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var jwt = settings.Value;
        options.Authority = jwt.Authority;
        options.Audience = jwt.Audience;
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters.ClockSkew = TimeSpan.FromMinutes(1);
    }
}
