using Bogus;

namespace RulesEngine.Benchmark;

public class Organization
{
    public int Id { get; set; }
    public string? OrganizationName { get; set; }

    public string? BankAccount { get; set; }
    public string? Phone { get; set; }

    public Address Address { get; set; } = null!;

    public List<Person> Employees { get; set; } = new List<Person>();

    private static int idSeed = 42;
    public static Faker<Organization> FakeData { get; } = new Faker<Organization>()
        .RuleFor(o => o.Id, f => idSeed++)
        .RuleFor(o => o.OrganizationName, f => f.Company.CompanyName())
        .RuleFor(o => o.BankAccount, f => f.Finance.Iban())
        .RuleFor(o => o.Address, f => Address.FakeData.Generate())
        .RuleFor(o => o.Employees, f => Person.FakeData.Generate(1000));
}
