namespace RulesEngine;

/// <summary>
/// A particular criterion that must be met in order for a rule to apply.
/// </summary>
public interface IRuleCriterion
{
    /// <summary>
    /// The property access path to evaluate on the object. For instance with an object Organization: "Ceo.Address.StreetAddress".
    /// </summary>
    public string PropertyPath { get; }

    /// <summary>
    /// The operator to compare the  
    /// </summary>
    public string OperatorName { get; }

    /// <summary>
    /// The value to compare the 
    /// </summary>
    public string Value { get; }
}
