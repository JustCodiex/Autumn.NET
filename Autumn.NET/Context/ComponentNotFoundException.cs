namespace Autumn.Context;

/// <summary>
/// Represents errors that occur when a requested component of a specific type is not found.
/// </summary>
public sealed class ComponentNotFoundException : AutumnException {

    /// <summary>
    /// Gets the type of the component that was requested and not found.
    /// </summary>
    public Type RequestedType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentNotFoundException"/> class with the type of the requested component.
    /// </summary>
    /// <param name="requestedType">The type of the component that was not found.</param>
    public ComponentNotFoundException(Type requestedType) : base($"Failed finding component of type {requestedType.FullName}") { 
        this.RequestedType = requestedType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentNotFoundException"/> class with the type of the requested component and a custom message.
    /// </summary>
    /// <param name="requestedType">The type of the component that was not found.</param>
    /// <param name="message">The message that describes the error.</param>
    public ComponentNotFoundException(Type requestedType, string message) : base(message) {
        this.RequestedType = requestedType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ComponentNotFoundException"/> class with the type of the requested component and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="requestedType">The type of the component that was not found.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public ComponentNotFoundException(Type requestedType, Exception innerException) : base($"Failed finding component of type {requestedType.FullName}", innerException) {
        this.RequestedType = requestedType;
    }

}
