namespace Autumn;

/// <summary>
/// Represents errors that occur within the Autumn framework.
/// This is a general-purpose exception class for the framework.
/// </summary>
public class AutumnException : Exception {

    /// <summary>
    /// Initializes a new instance of the <see cref="AutumnException"/> class with a default error message.
    /// </summary>
    public AutumnException() : base("Internal exception in the Autumn framework") {}

    /// <summary>
    /// Initializes a new instance of the <see cref="AutumnException"/> class with a custom error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public AutumnException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AutumnException"/> class with a custom error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public AutumnException(string message, Exception innerException) : base(message, innerException) { }

}
