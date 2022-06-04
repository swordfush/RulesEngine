namespace RulesEngine.Exceptions;

/// <summary>
/// An exception thrown when attempting to use the wrong datatype for the argument to an operation.
/// </summary>
public class InvalidArgumentTypeForOperatorException : Exception
{
    /// <summary>
    /// The name of the operator attempted.
    /// </summary>
    public string OperatorName { get; init; }

    /// <summary>
    /// The data type that the operator was attempted for.
    /// </summary>
    public Type DataType { get; init; }

    public InvalidArgumentTypeForOperatorException(string operatorName, Type dataType)
        : base($"Argument of type {dataType.FullName} is not valid for operator {operatorName}.")
    {
        this.OperatorName = operatorName;
        this.DataType = dataType;
    }
}
