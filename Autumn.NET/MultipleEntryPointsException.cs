namespace Autumn;

/// <summary>
/// The exception that is thrown when multiple entry points are detected.
/// </summary>
public class MultipleEntryPointsException : Exception {

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleEntryPointsException"/> class with the specified error message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public MultipleEntryPointsException(string message) : base(message) {}
}
