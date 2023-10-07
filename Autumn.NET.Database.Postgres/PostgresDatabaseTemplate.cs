using System.Reflection;
using System.Text;

using Autumn.Annotations;
using Autumn.Annotations.Library;
using Autumn.Database.Annotations;
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
        var rowmapper = GetRowMapperFromType<T>();
        return this.QueryInternal(sql, rowmapper, Array.Empty<object>());
    }


    /// <inheritdoc/>
    public override IList<T> Query<T>(string sql, params object?[] args) {
        var rowmapper = GetRowMapperFromType<T>();
        return this.QueryInternal(sql, rowmapper, args);
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

    private RowMapper<T> GetRowMapperFromType<T>() {
        if (!typeof(T).IsPrimitive) {
            return Context.GetRowMapper<T>();
        } else if (typeof(T) == typeof(int)) {
            return PostgresPrimitiveRowMapper.Int32 as RowMapper<T> ?? throw new Exception();
        } else if (typeof(T) == typeof(long)) {
            return PostgresPrimitiveRowMapper.Int64 as RowMapper<T> ?? throw new Exception();
        } else if (typeof(T) == typeof(bool)) {
            return PostgresPrimitiveRowMapper.Boolean as RowMapper<T> ?? throw new Exception();
        } else {
            throw new Exception();
        }
    }

    /// <inheritdoc/>
    public override T QueryForObject<T>(string sql) => QueryForObjectInternal(sql, GetRowMapperFromType<T>(), Array.Empty<object>());

    /// <inheritdoc/>
    public override T QueryForObject<T>(string sql, object?[] args) => QueryForObjectInternal(sql, GetRowMapperFromType<T>(), args);

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

    /// <inheritdoc/>
    public override int InsertObject<T>(T value) { // TODO: Optimize
        Type type = typeof(T);
        if (type.GetCustomAttribute<RowAttribute>() is not RowAttribute row) {
            throw new ArgumentException("Cannot insert non-row object", nameof(value));
        }
        if (string.IsNullOrEmpty(row.RelationName)) {
            throw new ArgumentException("Cannot insert row object into unknown relation");
        }
        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => (x, x.GetCustomAttribute<ColumnAttribute>()))
            .Where(x => x.Item2 is not null)
            .Where(x => x.Item2!.PrimaryKey is false)
            .ToArray();
        StringBuilder insertQuery = new StringBuilder("INSERT INTO ").Append(row.RelationName).Append(" (");
        StringBuilder parameterQuery = new StringBuilder("(");
        object?[] args = new object[props.Length];
        for (int i = 0; i < props.Length; i++) {
            insertQuery.Append(props[i].Item2!.ColumnName);
            parameterQuery.Append('?');
            if (i + 1 < props.Length) {
                insertQuery.Append(", ");
                parameterQuery.Append(", ");
            }
            args[i] = props[i].x.GetValue(value);
        }
        insertQuery.Append(") VALUES ").Append(parameterQuery).Append(')');
        return this.Update(insertQuery.ToString(), args);
    }

    /// <inheritdoc/>
    public override int UpsertObject<T>(T value) {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override int UpdateObject<T>(T value) {
        Type type = typeof(T);
        if (type.GetCustomAttribute<RowAttribute>() is not RowAttribute row) {
            throw new ArgumentException("Cannot update non-row object", nameof(value));
        }
        if (string.IsNullOrEmpty(row.RelationName)) {
            throw new ArgumentException("Cannot update row object into unknown relation");
        }
        var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => (x, x.GetCustomAttribute<ColumnAttribute>()))
            .Where(x => x.Item2 is not null)
            .Where(x => x.Item2!.PrimaryKey is false)
            .ToArray();
        var pk = type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => (x, x.GetCustomAttribute<ColumnAttribute>()))
            .Where(x => x.Item2 is not null)
            .Where(x => x.Item2!.PrimaryKey is true)
            .ToArray();
        if (pk.Length == 0) {
            throw new ArgumentException("Cannot update object with no primary key", nameof(value));
        } else if (pk.Length > 1) {
            throw new NotSupportedException("Cannot update object with multiple primary key columns");
        }
        StringBuilder insertQuery = new StringBuilder("UPDATE ").Append(row.RelationName).Append(" SET ");
        object?[] args = new object[props.Length+1];
        for (int i = 0; i < props.Length; i++) {
            insertQuery.Append(props[i].Item2!.ColumnName);
            insertQuery.Append(" = ");
            insertQuery.Append('?');
            if (i + 1 < props.Length) {
                insertQuery.Append(", ");
            }
            args[i] = props[i].x.GetValue(value);
        }
        insertQuery.Append(" WHERE ");
        insertQuery.Append(pk[0].Item2!.ColumnName);
        insertQuery.Append(" = ?");
        args[args.Length -1] = pk[0].x.GetValue(value);
        return this.Update(insertQuery.ToString(), args);
    }

}
