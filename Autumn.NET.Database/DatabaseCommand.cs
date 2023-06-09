namespace Autumn.Database;

/// <summary>
/// Represents an abstract database command.
/// </summary>
public abstract class DatabaseCommand : IDisposable {

    /// <summary>
    /// Executes the database command and returns the query result.
    /// </summary>
    /// <returns>The query result.</returns>
    public abstract QueryResult Execute();

    /// <summary>
    /// Executes the database command and returns the number of affected rows.
    /// </summary>
    /// <returns>The number of affected rows.</returns>
    public abstract int ExecuteUpdate();

    /// <summary>
    /// Sets the value of a parameter at the specified index.
    /// </summary>
    /// <param name="index">The index of the parameter.</param>
    /// <param name="value">The value to set.</param>
    public abstract void SetArgument(int index, object? value);

    /// <summary>
    /// Disposes the database command and releases any resources associated with it.
    /// </summary>
    public abstract void Dispose();

}
