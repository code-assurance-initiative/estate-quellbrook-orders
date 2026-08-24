using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Quellbrook.Orders.Infrastructure.Outbox;
using Quellbrook.Orders.Infrastructure.Persistence;
using Quellbrook.Orders.UnitTests.TestSupport;

namespace Quellbrook.Orders.UnitTests.Infrastructure;

public sealed class OutboxRelayTests : IDisposable
{
    private readonly SqliteOrdersDb _db = new();
    private readonly RecordingOutboxPublisher _publisher = new();
    private readonly FakeTimeProvider _time = new(OrderData.PlacedAt.AddMinutes(5));
    private readonly ServiceProvider _services;

    public OutboxRelayTests()
    {
        _services = new ServiceCollection().AddScoped(_ => _db.CreateContext()).BuildServiceProvider();
    }

    private OutboxRelay Relay(int batchSize = 50) =>
        new(_services.GetRequiredService<IServiceScopeFactory>(), _publisher, _time,
            Options.Create(new OutboxOptions { BatchSize = batchSize }), NullLogger<OutboxRelay>.Instance);

    [Fact]
    public async Task PendingMessagesArePublishedInOrderAndMarkedDispatched()
    {
        await StoreAsync(("orders.order-placed.v1", 1), ("orders.order-cancelled.v1", 2));

        var dispatched = await Relay().DispatchPendingAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, dispatched);
        Assert.Equal(["orders.order-placed.v1", "orders.order-cancelled.v1"], _publisher.Published.Select(message => message.EventType));
        using var context = _db.CreateContext();
        Assert.All(context.OutboxMessages, message => Assert.Equal(_time.GetUtcNow(), message.DispatchedAt));
    }

    [Fact]
    public async Task ADispatchedMessageIsNotPublishedAgain()
    {
        await StoreAsync(("orders.order-placed.v1", 1));
        await Relay().DispatchPendingAsync(TestContext.Current.CancellationToken);

        var again = await Relay().DispatchPendingAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, again);
        Assert.Single(_publisher.Published);
    }

    [Fact]
    public async Task AFailureStopsTheBatchKeepsOrderAndCountsTheAttempt()
    {
        await StoreAsync(("orders.order-placed.v1", 1), ("orders.order-cancelled.v1", 2), ("orders.order-placed.v1", 3));
        _publisher.FailOn = "orders.order-cancelled.v1";

        var dispatched = await Relay().DispatchPendingAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, dispatched);
        using var context = _db.CreateContext();
        var failed = context.OutboxMessages.Single(message => message.Type == "orders.order-cancelled.v1");
        Assert.Null(failed.DispatchedAt);
        Assert.Equal(1, failed.Attempts);
        Assert.Equal(1, context.OutboxMessages.Count(message => message.DispatchedAt != null));
    }

    [Fact]
    public async Task OneBatchTakesAtMostBatchSizeMessages()
    {
        await StoreAsync(("orders.order-placed.v1", 1), ("orders.order-placed.v1", 2), ("orders.order-placed.v1", 3));

        var dispatched = await Relay(batchSize: 2).DispatchPendingAsync(TestContext.Current.CancellationToken);

        Assert.Equal(2, dispatched);
    }

    [Fact]
    public async Task DispatchedMessagesOlderThanTheRetentionAreDeletedAndPendingOnesKept()
    {
        await StoreAsync(("orders.order-placed.v1", 1), ("orders.order-placed.v1", 2));
        await Relay(batchSize: 1).DispatchPendingAsync(TestContext.Current.CancellationToken);

        _time.Advance(TimeSpan.FromDays(6));
        var early = await Relay().PurgeExpiredAsync(TestContext.Current.CancellationToken);
        _time.Advance(TimeSpan.FromDays(2));
        var due = await Relay().PurgeExpiredAsync(TestContext.Current.CancellationToken);

        Assert.Equal(0, early);
        Assert.Equal(1, due);
        using var context = _db.CreateContext();
        var left = Assert.Single(context.OutboxMessages);
        Assert.Null(left.DispatchedAt);
    }

    private async Task StoreAsync(params (string Type, int Minute)[] messages)
    {
        using var context = _db.CreateContext();
        foreach (var (type, minute) in messages)
        {
            context.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = type,
                Payload = "{}",
                OccurredAt = OrderData.PlacedAt.AddMinutes(minute),
            });
        }

        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    public void Dispose()
    {
        _services.Dispose();
        _db.Dispose();
    }
}
