using Bogus;

namespace RulesEngine.Benchmark;

public class Person
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Title { get; set; }
    public DateTime? DOB { get; set; }
    public string Email { get; set; } = null!;
    public string? Phone { get; set; }

    public Address Address { get; set; } = null!;

    private static int idSeed = 100;
    public static Faker<Person> FakeData { get; } = new Faker<Person>()
        .RuleFor(p => p.Id, f => idSeed++)
        .RuleFor(p => p.FirstName, f => f.Name.FirstName())
        .RuleFor(p => p.MiddleName, f => f.Name.FirstName())
        .RuleFor(p => p.LastName, f => f.Name.LastName())
        .RuleFor(p => p.Title, f => f.Name.Prefix(f.Person.Gender))
        .RuleFor(p => p.Email, (f, p) => f.Internet.Email(p.FirstName, p.LastName))
        .RuleFor(p => p.DOB, f => f.Date.Past(18))
        .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber("(###)-###-####"))
        .RuleFor(p => p.Address, f => Address.FakeData.Generate());
}
