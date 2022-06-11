namespace RulesEngine.Tests.ConcreteImplementation;

class RuleCriterion : IRuleCriterion
{
    public string PropertyPath { get; set; }
    public string OperatorName { get; set; }
    public string Value { get; set; }

    public RuleCriterion(string propertyPath, string operatorName, string value)
    {
        PropertyPath = propertyPath;
        OperatorName = operatorName;
        Value = value;
    }
}
