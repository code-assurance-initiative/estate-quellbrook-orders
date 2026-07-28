using Quellbrook.Orders.Domain.Common;

namespace Quellbrook.Orders.Domain.Orders;

/// <summary>The person or business the parcels are delivered to.</summary>
public sealed record Consignee
{
    private Consignee(string name, Address address, ContactDetails contact)
    {
        Name = name;
        Address = address;
        Contact = contact;
    }

    public string Name { get; }

    public Address Address { get; }

    public ContactDetails Contact { get; }

    public static Consignee Create(string name, Address address, ContactDetails contact)
    {
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(contact);
        return new Consignee(Text.Required(name, 100, nameof(name)), address, contact);
    }
}
