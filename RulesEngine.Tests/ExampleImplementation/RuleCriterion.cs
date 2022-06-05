namespace RulesEngine.Tests.ExampleImplementation;

class RuleCriterion : IRuleCriterion
{
    public string PropertyPath { get; set; } = null!;
    public string OperatorName { get; set; } = null!;
    public string Value { get; set; } = null!;
}
