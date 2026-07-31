using Microsoft.AspNetCore.Authentication.JwtBearer;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Quellbrook.Orders.Api.Security;
using Quellbrook.Orders.Application;
using Quellbrook.Orders.Infrastructure;

namespace Quellbrook.Orders.Api.Hosting;

public static class ApiServices
{
    public static WebApplicationBuilder AddOrdersApi(this WebApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var services = builder.Services;

        services.AddSingleton(TimeProvider.System);
        services.AddOrdersApplication();
        services.AddOrdersInfrastructure(builder.Configuration);
        services.AddProblemDetails();

        services.AddOptions<JwtAuthenticationOptions>()
            .BindConfiguration(JwtAuthenticationOptions.SectionName)
            .ValidateDataAnnotations()
            .Validate(options => options.HasHttpsAuthority(), "Authentication:Authority must be an absolute https URL.")
            .ValidateOnStart();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.ConfigureOptions<ConfigureJwtBearerOptions>();
        services.AddAuthorizationBuilder().AddOrdersPolicies();

        builder.Logging.AddOpenTelemetry(logging => logging.IncludeScopes = true);
        var telemetry = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("quellbrook-orders"))
            .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation())
            .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation());
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            telemetry.UseOtlpExporter();
        }

        return builder;
    }
}
