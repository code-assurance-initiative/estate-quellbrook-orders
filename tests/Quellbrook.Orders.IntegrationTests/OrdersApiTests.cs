using System.Net;
using System.Net.Http.Json;
using Quellbrook.Orders.Contracts.IntegrationEvents;

namespace Quellbrook.Orders.IntegrationTests;

public sealed class OrdersApiTests(OrdersApiFactory factory) : IClassFixture<OrdersApiFactory>
{
    private static object NewOrder(string serviceLevel = "standard") => new
    {
        customerAccountId = "QB-104233",
        serviceLevel,
        consignee = new
        {
            name = "Halden Bikes ApS",
            line1 = "Søndergade 12",
            postalCode = "8000",
            city = "Aarhus C",
            countryCode = "DK",
            email = "orders@halden-bikes.example",
        },
        parcels = new[] { new { weightGrams = 2400, lengthCm = 40, widthCm = 30, heightCm = 20 } },
    };

    [Fact]
    public async Task APlacedOrderCanBeReadBackAndIsAnnounced()
    {
        var client = factory.CreateClient("operator-17", "orders:read", "orders:write");

        var created = await client.PostAsJsonAsync("/orders", NewOrder(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var order = await client.GetFromJsonAsync<OrderDto>(created.Headers.Location, TestContext.Current.CancellationToken);
        Assert.NotNull(order);
        Assert.Equal("placed", order.Status);
        Assert.Equal("operator-17", order.PlacedBy);
        Assert.Equal(2400, order.TotalWeightGrams);
        Assert.Contains(factory.Publisher.Published, message =>
            message.EventType == OrderPlacedV1.EventType && ((OrderPlacedV1)message.Payload).OrderId == order.Id);
    }

    [Fact]
    public async Task OrdersAreListedAPageAtATime()
    {
        var client = factory.CreateClient("operator-17", "orders:read", "orders:write");
        await client.PostAsJsonAsync("/orders", NewOrder(), TestContext.Current.CancellationToken);

        var page = await client.GetFromJsonAsync<PageDto>("/orders?page=1&pageSize=10", TestContext.Current.CancellationToken);

        Assert.NotNull(page);
        Assert.NotEmpty(page.Items);
        Assert.Equal(10, page.PageSize);
    }

    [Fact]
    public async Task AnOversizedPageIsRefused()
    {
        var client = factory.CreateClient("operator-17", "orders:read");

        var response = await client.GetAsync("/orders?pageSize=500", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AnIncompleteOrderIsAValidationProblem()
    {
        var client = factory.CreateClient("operator-17", "orders:write");

        var response = await client.PostAsJsonAsync("/orders", new { customerAccountId = "QB-104233" }, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("consignee", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ABusinessRuleViolationIsUnprocessable()
    {
        var client = factory.CreateClient("operator-17", "orders:write");

        var response = await client.PostAsJsonAsync("/orders", NewOrder(serviceLevel: "overnight"), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task PlacingAnOrderNeedsTheOperatorHeader()
    {
        var client = factory.CreateClient(null, "orders:write");

        var response = await client.PostAsJsonAsync("/orders", NewOrder(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AnUnknownOrderIsNotFound()
    {
        var client = factory.CreateClient("operator-17", "orders:read");

        var response = await client.GetAsync($"/orders/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task WithoutATokenTheApiAnswers401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/orders", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReadingScopeCannotPlaceOrders()
    {
        var client = factory.CreateClient("operator-17", "orders:read");

        var response = await client.PostAsJsonAsync("/orders", NewOrder(), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private sealed record OrderDto(Guid Id, string Status, string PlacedBy, int TotalWeightGrams);

    private sealed record PageDto(IReadOnlyList<object> Items, int Page, int PageSize, int TotalCount);
}
