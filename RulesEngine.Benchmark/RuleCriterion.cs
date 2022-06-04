using Bogus;
using System.Reflection;

namespace RulesEngine.Benchmark;

public class RuleCriterion : IRuleCriterion
{
    public string PropertyPath { get; set; }
    public string OperatorName { get; set; }
    public string Value { get; set; }

    public RuleCriterion()
    {
        PropertyPath = null!;
        OperatorName = null!;
        Value = null!;
    }

    public RuleCriterion(string propertyName, string operatorName, string value)
    {
        PropertyPath = propertyName;
        OperatorName = operatorName;
        Value = value;
    }

    private static List<PropertyInfo> GetValueProperties(Type type)
        => type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.PropertyType.IsValueType).ToList();

    private static List<string> GetPropertyPaths()
    {
        var paths = new List<string>();

        foreach (var property in GetValueProperties(typeof(Organization)))
            paths.Add($"{property.Name}");

        foreach (var property in GetValueProperties(typeof(Address)))
            paths.Add($"{nameof(Organization.Address)}.{property.Name}");

        return paths;
    }

    public static Faker<RuleCriterion> FakeData { get; } = new Faker<RuleCriterion>()
        .RuleFor(r => r.PropertyPath, f => f.PickRandom(GetPropertyPaths()))
        .RuleFor(r => r.OperatorName, f => "Equal")
        .RuleFor(r => r.Value, f => "42");
}
