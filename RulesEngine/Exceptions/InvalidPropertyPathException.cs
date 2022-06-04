namespace RulesEngine.Exceptions;

/// <summary>
/// An exception thrown when attempting to access a series of properties that do not exist on the object.
/// For instance, attempting to access <c>Patient.Demographics.Ethnicity</c> where <c>Patient</c> has no <c>Demographics</c> property.
/// </summary>
public class InvalidPropertyPathException : Exception
{
    /// <summary>
    /// The type of the object that the property path was evaluated against.
    /// </summary>
    public Type ObjectType { get; set; }

    /// <summary>
    /// The property path attempted.
    /// </summary>
    public string PropertyPath { get; init; }

    public InvalidPropertyPathException(Type objectType, string propertyPath, string invalidPropertyName, Type inspectedObjectType)
        : base($"Invalid property path '{propertyPath}' for type {objectType.FullName}. Property {invalidPropertyName} does not exist on type {inspectedObjectType.FullName}.")
    {
        this.ObjectType = objectType;
        this.PropertyPath = propertyPath;
    }
}
