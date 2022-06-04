namespace RulesEngine;

/// <summary>
/// Indicates an issue with a rule definition.
/// </summary>
public class RuleValidationError
{
    /// <summary>
    /// The reason why the rule is invalid. 
    /// </summary>
    public string ErrorMessage { get; init; }

    public RuleValidationError(string errorMessage)
    {
        this.ErrorMessage = errorMessage;
    }

    public override string ToString()
        => this.ErrorMessage;
}
