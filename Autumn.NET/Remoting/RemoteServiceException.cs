namespace Autumn.Remoting;

/// <summary>
/// Represents an exception that occurs when interacting with a remote service.
/// </summary>
public class RemoteServiceException : AutumnException {

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteServiceException"/> class.
    /// </summary>
    public RemoteServiceException()
        : base("An error occurred while interacting with the remote service.") {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteServiceException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public RemoteServiceException(string message)
        : base(message) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteServiceException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public RemoteServiceException(string message, Exception innerException)
        : base(message, innerException) {
    }

}
