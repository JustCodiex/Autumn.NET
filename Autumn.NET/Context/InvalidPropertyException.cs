namespace Autumn.Context;

/// <summary>
/// Represents errors that occur when the value of a property was invalid.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="InvalidPropertyException"/> class with the property path and value that caused the exception.
/// </remarks>
/// <param name="propertyPath">The path to the property that was invalid.</param>
/// <param name="propertyValue">The value of the property that was invalid.</param>
/// <param name="message">The message to display.</param>
public sealed class InvalidPropertyException(string propertyPath, string propertyValue, string message) : AutumnException(message) {

    /// <summary>
    /// Gets the path of the property that was invalid.
    /// </summary>
    public string PropertyPath { get; } = propertyPath;

    /// <summary>
    /// Gets the value of the property.
    /// </summary>
    public string PropertyValue { get; } = propertyValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPropertyException"/> class with the property path and value that caused the exception.
    /// </summary>
    /// <param name="propertyPath">The path to the property that was invalid.</param>
    /// <param name="propertyValue">The value of the property that was invalid.</param>
    public InvalidPropertyException(string propertyPath, string propertyValue) : this(propertyPath, propertyValue, $"Invalid property '{propertyPath}' with value '{propertyValue}'") { }

}
