namespace Autumn.Database.Relational;

/// <summary>
/// Represents a delegate for mapping a database query result to an entity object of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the entity object.</typeparam>
/// <param name="result">The query result.</param>
/// <param name="rowIndex">The index of the current row in the query result.</param>
/// <returns>The mapped entity object.</returns>
public delegate T RowMapper<T>(QueryResult result, int rowIndex);

/// <summary>
/// Represents a delegate for the generated row mapper used internally in the <see cref="DatabaseApplicationContext"/> class.
/// </summary>
/// <param name="result">The query result.</param>
/// <param name="rowIndex">The index of the current row in the query result.</param>
/// <returns>The mapped entity object.</returns>
public delegate object GeneratedRowMapper(QueryResult result, int rowIndex);
