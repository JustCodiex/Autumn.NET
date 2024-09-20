namespace Autumn;

/// <summary>
/// Exception thrown when the application or appContext is not properly initialized.
/// </summary>
public class ApplicationInitializationException : AutumnException {

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationInitializationException"/> class.
    /// </summary>
    public ApplicationInitializationException() : base("The application or application context is not properly initialized.") {}

    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationInitializationException"/> class.
    /// </summary>
    /// <param name="message">The detailed message describing the exception.</param>
    public ApplicationInitializationException(string message) : base(message) { }

}
