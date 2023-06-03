using Autumn.Annotations.Internal;

namespace Autumn.Arguments;

/// <summary>
/// Represents command-line arguments for the application.
/// </summary>
[InternalComponent]
public class CommandLineArgs : ICommandLineArguments {

    protected readonly string[] args;
    protected IDictionary<string, object> values;

    /// <summary>
    /// Gets the source of the command-line arguments as a single string.
    /// </summary>
    public string Source => string.Join(' ', args);

    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgs"/> class with the specified arguments.
    /// </summary>
    /// <param name="arguments">The command-line arguments.</param>
    public CommandLineArgs(string[] arguments) {
        this.args = arguments;
        this.values = new Dictionary<string, object>();
    }

    /// <summary>
    /// Parses the command-line arguments and populates the argument values.
    /// </summary>
    /// <returns>True if the parsing is successful; otherwise, false.</returns>
    public virtual bool Parse() {
        for (int i = 0; i < args.Length; i++) {
            if (args[i].StartsWith('-')) {
                string nextArg = i + 1 < args.Length ? args[i+1] : bool.TrueString;
                if (nextArg == bool.TrueString || nextArg.StartsWith('-')) {
                    values.Add(args[i], bool.TrueString);
                } else {
                    values.Add(args[i], nextArg);
                    i++;
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Checks if the specified argument is present in the command-line arguments.
    /// </summary>
    /// <param name="argument">The argument to check.</param>
    /// <returns>True if the argument is present; otherwise, false.</returns>
    public virtual bool HasArgument(string argument) => values.ContainsKey(argument);

    /// <summary>
    /// Gets the value of the specified argument from the command-line arguments.
    /// </summary>
    /// <typeparam name="T">The type of the argument value.</typeparam>
    /// <param name="argument">The argument to retrieve the value for.</param>
    /// <returns>The value of the argument.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the argument is not present in the command-line arguments.</exception>
    /// <exception cref="Exception">Thrown if the value cannot be cast to the specified type.</exception>
    public virtual T GetArgumentValue<T>(string argument) {
        if (!values.TryGetValue(argument, out object? value)) { 
            throw new ArgumentNullException(argument);
        }
        if (value is T t) {
            return t;
        }
        throw new Exception();
    }

}
