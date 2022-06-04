namespace RulesEngine;

/// <summary>
/// A particular criterion that must be met in order for a rule to apply.
/// </summary>
public interface IRuleCriterion
{
    public string PropertyName { get; }
    public string OperatorName { get; }
    public string Value { get; }
}
