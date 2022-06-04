namespace RulesEngine.Tests.ConcreteImplementation;

class RuleCriterion : IRuleCriterion
{
    public string PropertyPath { get; set; }
    public string OperatorName { get; set; }
    public string Value { get; set; }

    public RuleCriterion(string propertyName, string operatorName, string value)
    {
        PropertyPath = propertyName;
        OperatorName = operatorName;
        Value = value;
    }
}
