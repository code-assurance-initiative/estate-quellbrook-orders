using Quellbrook.Orders.Api.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.AddOrdersApi();

var app = builder.Build();
app.UseOrdersApi();
await app.RunAsync().ConfigureAwait(false);

/// <summary>Entry point; public so the integration tests can host the API.</summary>
public partial class Program;
