namespace Autumn;

/// <summary>
/// The exception that is thrown when multiple entry points are detected.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MultipleEntryPointsException"/> class with the specified error message.
/// </remarks>
/// <param name="message">The error message that explains the reason for the exception.</param>
public sealed class MultipleEntryPointsException(string message) : AutumnException(message);
