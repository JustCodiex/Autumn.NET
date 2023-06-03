namespace Autumn.Arguments;

/// <summary>
/// Represents command-line arguments.
/// </summary>
public interface ICommandLineArguments {

    /// <summary>
    /// Gets the source of the command-line arguments as a single string.
    /// </summary>
    string Source { get; }

    /// <summary>
    /// Parses the command-line arguments and populates the argument values.
    /// </summary>
    /// <returns>True if the parsing is successful; otherwise, false.</returns>
    bool Parse();

    /// <summary>
    /// Checks if the specified argument is present in the command-line arguments.
    /// </summary>
    /// <param name="argument">The argument to check.</param>
    /// <returns>True if the argument is present; otherwise, false.</returns>
    bool HasArgument(string argument);

    /// <summary>
    /// Gets the value of the specified argument from the command-line arguments.
    /// </summary>
    /// <typeparam name="T">The type of the argument value.</typeparam>
    /// <param name="argument">The argument to retrieve the value for.</param>
    /// <returns>The value of the argument.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the argument is not present in the command-line arguments.</exception>
    /// <exception cref="Exception">Thrown if the value cannot be cast to the specified type.</exception>
    T GetArgumentValue<T>(string argument);

}
