using Autumn.Annotations.Library;
using Autumn.Database.Relational;

namespace Autumn.Database;

/// <summary>
/// Represents an abstract database template, providing a common contract for database-related functionality.
/// </summary>
[AutumnTemplate]
public abstract class DatabaseTemplate {

    /// <summary>
    /// Executes a SQL query and returns a single result object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>The single result object.</returns>
    public abstract T QueryForObject<T>(string sql);

    /// <summary>
    /// Executes a parameterized SQL query and returns a single result object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="args">The arguments for the parameterized SQL query.</param>
    /// <returns>The single result object.</returns>
    public abstract T QueryForObject<T>(string sql, params object?[] args);

    /// <summary>
    /// Executes a SQL query and returns a single result object of type <typeparamref name="T"/> using the provided <paramref name="mapper"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="mapper">The row mapper function to map the result object.</param>
    /// <returns>The single result object.</returns>
    public abstract T QueryForObject<T>(string sql, RowMapper<T> mapper);

    /// <summary>
    /// Executes a parameterized SQL query and returns a single result object of type <typeparamref name="T"/> using the provided <paramref name="mapper"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result object.</typeparam>
    /// <param name="sql">The parameterized SQL query to execute.</param>
    /// <param name="mapper">The row mapper function to map the result object.</param>
    /// <param name="args">The arguments for the parameterized SQL query.</param>
    /// <returns>The single result object.</returns>
    public abstract T QueryForObject<T>(string sql, RowMapper<T> mapper, params object?[] args);

    /// <summary>
    /// Executes a SQL query and returns a list of result objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result objects.</typeparam>
    /// <param name="sql">The SQL query to execute.</param>
    /// <returns>The list of result objects.</returns>
    public abstract IList<T> Query<T>(string sql);

    /// <summary>
    /// Executes a parameterized SQL query and returns a list of result objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result objects.</typeparam>
    /// <param name="sql">The parameterized SQL query to execute.</param>
    /// <param name="args">The arguments for the parameterized SQL query.</param>
    /// <returns>The list of result objects.</returns>
    public abstract IList<T> Query<T>(string sql, params object?[] args);

    /// <summary>
    /// Executes a SQL query and returns a list of result objects of type <typeparamref name="T"/> using the provided <paramref name="mapper"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result objects.</typeparam>
    /// <param name="sql">The SQL query to execute.</param>
    /// <param name="mapper">The row mapper function to map the result objects.</param>
    /// <returns>The list of result objects.</returns>
    public abstract IList<T> Query<T>(string sql, RowMapper<T> mapper);

    /// <summary>
    /// Executes a parameterized SQL query and returns a list of result objects of type <typeparamref name="T"/> using the provided <paramref name="mapper"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result objects.</typeparam>
    /// <param name="sql">The parameterized SQL query to execute.</param>
    /// <param name="mapper">The row mapper function to map the result objects.</param>
    /// <param name="args">The arguments for the parameterized SQL query.</param>
    /// <returns>The list of result objects.</returns>
    public abstract IList<T> Query<T>(string sql, RowMapper<T> mapper, params object?[] args);

    /// <summary>
    /// Executes a SQL update statement and returns the number of affected rows.
    /// </summary>
    /// <param name="sql">The SQL update statement to execute.</param>
    /// <returns>The number of affected rows.</returns>
    public abstract int Update(string sql);

    /// <summary>
    /// Executes a parameterized SQL update statement and returns the number of affected rows.
    /// </summary>
    /// <param name="sql">The parameterized SQL update statement to execute.</param>
    /// <param name="args">The arguments for the parameterized SQL update statement.</param>
    /// <returns>The number of affected rows.</returns>
    public abstract int Update(string sql, params object?[] args);

    /// <summary>
    /// Inserts the value into the database using 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public abstract int InsertObject<T>(T value);

}
