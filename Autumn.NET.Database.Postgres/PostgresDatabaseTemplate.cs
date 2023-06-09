using Autumn.Annotations;
using Autumn.Annotations.Library;
using Autumn.Database.Lib;
using Autumn.Database.Relational;

namespace Autumn.Database.Postgres;

/// <summary>
/// Represents a concrete implementation of the <see cref="DatabaseTemplate"/> for a Postgres database.
/// </summary>
[AutumnTemplateImplementation(typeof(DatabaseTemplate))]
public class PostgresDatabaseTemplate : DatabaseTemplate {

    private readonly PostgresDataSource dataSource;

    /// <summary>
    /// Gets the application database context.
    /// </summary>
    [Inject]
    public DatabaseApplicationContext Context { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresDatabaseTemplate"/> class with the specified Postgres data source.
    /// </summary>
    /// <param name="dataSource">The Postgres data source.</param>
    public PostgresDatabaseTemplate([Inject] PostgresDataSource dataSource) {
        this.dataSource = dataSource;
    }

    /// <inheritdoc/>
    public override IList<T> Query<T>(string sql) {
        var rowmapper = this.Context.GetRowMapper<T>();
        return this.QueryInternal(sql, rowmapper, Array.Empty<object>());
    }

    /// <inheritdoc/>
    public override IList<T> Query<T>(string sql, RowMapper<T> mapper) 
        => this.QueryInternal(sql, mapper, Array.Empty<object>());

    /// <inheritdoc/>
    public override IList<T> Query<T>(string sql, RowMapper<T> mapper, params object?[] args)
        => this.QueryInternal(sql, mapper, args);

    private IList<T> QueryInternal<T>(string sql, RowMapper<T> mapper, object?[] arguments) {
        var result = new List<T>();
        using (var cmd = dataSource.CreateCommand(sql)) {
            for (int i = 0; i < arguments.Length; i++) {
                cmd.SetArgument(i, arguments[i]);
            }
            using (var reader = cmd.Execute()) {
                int counter = 0;
                while (reader.Read()) {
                    result.Add(mapper(reader, counter));
                    counter++;
                }
            }
        }
        return result;
    }

    /// <inheritdoc/>
    public override T QueryForObject<T>(string sql) => QueryForObjectInternal(sql, Context.GetRowMapper<T>(), Array.Empty<object>());

    /// <inheritdoc/>
    public override T QueryForObject<T>(string sql, RowMapper<T> mapper) => QueryForObjectInternal(sql, mapper, Array.Empty<object>());

    /// <inheritdoc/>
    public override T QueryForObject<T>(string sql, RowMapper<T> mapper, params object?[] args) => QueryForObjectInternal(sql, mapper, args);

    private T QueryForObjectInternal<T>(string sql, RowMapper<T> mapper, object?[] arguments) {
        using (var cmd = dataSource.CreateCommand(sql)) {
            for (int i = 0; i < arguments.Length; i++) {
                cmd.SetArgument(i, arguments[i]);
            }
            using (var reader = cmd.Execute()) {
                if (!reader.Read()) {
                    throw new Exception();
                }
                var result = mapper(reader, 0);
                if (reader.Read()) {
                    throw new Exception();
                }
                return result;
            }
        }
    }

    /// <inheritdoc/>
    public override int Update(string sql) => UpdateInternal(sql, Array.Empty<object>());

    /// <inheritdoc/>
    public override int Update(string sql, params object?[] args) => UpdateInternal(sql, args);

    private int UpdateInternal(string sql, object?[] arguments) {
        int result = 0;
        using (var cmd = dataSource.CreateCommand(sql)) {
            for (int i = 0; i < arguments.Length; i++) {
                cmd.SetArgument(i, arguments[i]);
            }
            result = cmd.ExecuteUpdate();
        }
         return result;
    }

}
