using Bogus;

namespace RulesEngine.Benchmark;

public class Address
{
    public string? StreetAddress { get; set; }
    public string? Suburb { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostCode { get; set; }

    public static Faker<Address> FakeData { get; } = new Faker<Address>()
        .RuleFor(a => a.StreetAddress, f => f.Address.StreetAddress())
        .RuleFor(a => a.City, f => f.Address.City())
        .RuleFor(a => a.Country, f => f.Address.Country())
        .RuleFor(a => a.PostCode, f => f.Address.ZipCode());
}
