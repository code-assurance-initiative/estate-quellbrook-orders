using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Quellbrook.Orders.Api.Endpoints;
using Quellbrook.Orders.Api.Security;

namespace Quellbrook.Orders.Api.Hosting;

public static class ApiPipeline
{
    public static WebApplication UseOrdersApi(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        // In-cluster traffic reaches the service through the mesh sidecar, which terminates mutual TLS
        // (docs/architecture.md); the service itself speaks plain HTTP to it and is never exposed outside the cluster.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler();
        }

        app.UseStatusCodePages();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") })
            .AllowAnonymous();
        app.MapOrderEndpoints();
        return app;
    }
}
