namespace RulesEngine;

/// <summary>
/// A rule that objects can either adhere to, or not.
/// </summary>
public interface IRule
{
    public IEnumerable<IRuleCriterion> Criteria { get; }
}
