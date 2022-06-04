namespace RulesEngine.Exceptions;

/// <summary>
/// An exception thrown when attempting to use an unrecognised operator.
/// </summary>
public class UnrecognizedOperatorException : Exception
{
    /// <summary>
    /// The name of the operator attempted.
    /// </summary>
    public string OperatorName { get; init; }

    public UnrecognizedOperatorException(string operatorName)
        : base($"Unrecognized operator {operatorName}.")
    {
        this.OperatorName = operatorName;
    }
}
