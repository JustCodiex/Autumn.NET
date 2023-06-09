using Autumn.Annotations.Library;

namespace Autumn.Database;

/// <summary>
/// Represents a data source for a database.
/// </summary>
[AutumnTemplate]
public abstract class DataSource {

    /// <summary>
    /// Creates a database command object for the specified SQL command.
    /// </summary>
    /// <param name="command">The SQL command.</param>
    /// <returns>A database command object.</returns>
    public abstract DatabaseCommand CreateCommand(string command);

}
