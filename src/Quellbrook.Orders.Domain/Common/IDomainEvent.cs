namespace Quellbrook.Orders.Domain.Common;

/// <summary>Something that happened to an aggregate, in the past tense.</summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
