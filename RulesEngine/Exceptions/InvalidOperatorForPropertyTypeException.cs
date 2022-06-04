namespace RulesEngine.Exceptions;

/// <summary>
/// An exception thrown when attempting to use an operator that is not applicable/valid for the property datatype.
/// </summary>
public class InvalidOperatorForPropertyTypeException : Exception
{
    /// <summary>
    /// The name of the operator attempted.
    /// </summary>
    public string OperatorName { get; init; }

    /// <summary>
    /// The data type that the operator was attempted for.
    /// </summary>
    public Type DataType { get; init; }

    public InvalidOperatorForPropertyTypeException(string operatorName, Type dataType)
        : base($"Operator {operatorName} is not valid for property type {dataType.FullName}.")
    {
        this.OperatorName = operatorName;
        this.DataType = dataType;
    }
}
