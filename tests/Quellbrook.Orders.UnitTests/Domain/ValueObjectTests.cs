using Quellbrook.Orders.Domain.Common;
using Quellbrook.Orders.Domain.Orders;

namespace Quellbrook.Orders.UnitTests.Domain;

public sealed class ValueObjectTests
{
    [Theory]
    [InlineData("QB-1042")]
    [InlineData("QB-104233891234")]
    public void CustomerAccountNumbersHaveFourToTwelveDigits(string value) =>
        Assert.Equal(value, CustomerAccountId.Parse(value).Value);

    [Theory]
    [InlineData("QB-104")]
    [InlineData("QB-1042338912345")]
    [InlineData("XX-104233")]
    [InlineData("QB-10A233")]
    [InlineData("")]
    public void AnythingElseIsNotACustomerAccountNumber(string value) =>
        Assert.Throws<DomainException>(() => CustomerAccountId.Parse(value));

    [Fact]
    public void AddressesAreTrimmedAndNormalised()
    {
        var address = Address.Create("  Søndergade 12 ", " ", "8000", "Aarhus C", "dk");

        Assert.Equal("Søndergade 12", address.Line1);
        Assert.Null(address.Line2);
        Assert.Equal("DK", address.CountryCode);
    }

    [Fact]
    public void QuellbrookDeliversOnlyToItsCountries() =>
        Assert.Throws<DomainException>(() => Address.Create("Main Street 1", null, "10001", "New York", "US"));

    [Fact]
    public void AMissingCityIsRefused() =>
        Assert.Throws<DomainException>(() => Address.Create("Søndergade 12", null, "8000", " ", "DK"));

    [Fact]
    public void OverlongTextIsRefused() =>
        Assert.Throws<DomainException>(() => Address.Create(new string('x', 101), null, "8000", "Aarhus C", "DK"));

    [Theory]
    [InlineData(0, 10, 10)]
    [InlineData(10, 176, 10)]
    public void EverySideOfAParcelIsBetween1And175Cm(int length, int width, int height) =>
        Assert.Throws<DomainException>(() => Dimensions.Create(length, width, height));

    [Fact]
    public void ContactDetailsAreOptional()
    {
        var contact = ContactDetails.Create(" ", null);

        Assert.Null(contact.Email);
        Assert.Null(contact.Phone);
        Assert.Equal(ContactDetails.None, contact);
    }

    [Fact]
    public void PhoneNumbersAreStoredInInternationalForm() =>
        Assert.Equal("+4520304050", ContactDetails.Create(null, "+45 20 30-40-50").Phone);

    [Theory]
    [InlineData("20304050")]
    [InlineData("+45 2030 40x0")]
    [InlineData("+0045203040")]
    public void ANationalOrMalformedPhoneNumberIsRefused(string phone) =>
        Assert.Throws<DomainException>(() => ContactDetails.Create(null, phone));

    [Theory]
    [InlineData("orders@halden-bikes")]
    [InlineData("Halden <orders@halden-bikes.example>")]
    [InlineData("not an address")]
    public void AMalformedEmailAddressIsRefused(string email) =>
        Assert.Throws<DomainException>(() => ContactDetails.Create(email, null));

    [Fact]
    public void AWellFormedEmailAddressIsKept() =>
        Assert.Equal("orders@halden-bikes.example", ContactDetails.Create("orders@halden-bikes.example", null).Email);
}
