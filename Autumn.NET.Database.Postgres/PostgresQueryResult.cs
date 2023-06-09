using Npgsql;

namespace Autumn.Database.Postgres;

/// <summary>
/// Represents the query result from a PostgreSQL database query.
/// </summary>
public sealed class PostgresQueryResult : QueryResult {

    private readonly NpgsqlDataReader reader;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresQueryResult"/> class.
    /// </summary>
    /// <param name="dataReader">The NpgsqlDataReader containing the query result.</param>
    public PostgresQueryResult(NpgsqlDataReader dataReader) { 
        this.reader = dataReader;
    }

    /// <summary>
    /// Releases the resources used by the query result.
    /// </summary>
    public override void Dispose() {
        reader.Dispose();
    }

    /// <summary>
    /// Gets the index of the column with the specified name.
    /// </summary>
    /// <param name="name">The name of the column.</param>
    /// <returns>The index of the column.</returns>
    public override int GetColumnIndex(string name) 
        => reader.GetOrdinal(name);

    /// <summary>
    /// Gets the value of the column at the specified index, converted to the specified type.
    /// </summary>
    /// <param name="type">The type to which the column value should be converted.</param>
    /// <param name="columnIndex">The index of the column.</param>
    /// <returns>The value of the column.</returns>
    public override object? GetColumnValue(Type type, int columnIndex) {
        if (type == typeof(string)) {
            return reader.GetString(columnIndex);
        } else if (type == typeof(int)) {
            return reader.GetInt32(columnIndex);
        } else if (type == typeof(decimal)) {
            return (decimal)reader.GetDouble(columnIndex);
        } else if (type == typeof(DateOnly)) {
            return DateOnly.FromDateTime(reader.GetDateTime(columnIndex));
        }
        throw new NotSupportedException();
    }

    /// <summary>
    /// Advances to the next row in the query result.
    /// </summary>
    /// <returns><c>true</c> if there is another row to read; otherwise, <c>false</c>.</returns>
    public override bool Read() => reader.Read();

}
