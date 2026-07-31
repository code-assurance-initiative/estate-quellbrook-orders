namespace Quellbrook.Orders.Api.Security;

/// <summary>
/// Response headers that tell browsers to treat every response as inert data: nothing may be framed, sniffed into
/// another content type, or used to load further resources.
/// </summary>
public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var headers = context.Response.Headers;
        headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Cross-Origin-Resource-Policy"] = "same-origin";
        return next(context);
    }
}
