namespace Autumn.Database;

/// <summary>
/// Represents a query result from a database query.
/// </summary>
public abstract class QueryResult : IDisposable {

    /// <summary>
    /// Reads the next row from the query result.
    /// </summary>
    /// <returns><c>true</c> if there is another row to read; otherwise, <c>false</c>.</returns>
    public abstract bool Read();

    /// <summary>
    /// Releases the resources used by the query result.
    /// </summary>
    public abstract void Dispose();

    /// <summary>
    /// Gets the index of the column with the specified name.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <returns>The index of the column.</returns>
    public abstract int GetColumnIndex(string name);

    /// <summary>
    /// Gets the value of the column at the specified index, converted to the specified type.
    /// </summary>
    /// <param name="type">The type to which the column value should be converted.</param>
    /// <param name="columnIndex">The index of the column.</param>
    /// <returns>The value of the column.</returns>
    public abstract object? GetColumnValue(Type type, int columnIndex);

}
