using Autumn.Annotations;
using Autumn.Annotations.Library;
using Autumn.Database.Lib;
using Autumn.Database.Relational;

namespace Autumn.Database.Postgres;

[AutumnTemplateImplementation(typeof(DatabaseTemplate))]
public class PostgresDatabaseTemplate : DatabaseTemplate {

    private readonly PostgresDataSource dataSource;

    [Inject]
    public DatabaseLibraryContext Context { get; }

    public PostgresDatabaseTemplate([Inject] PostgresDataSource dataSource) {
        this.dataSource = dataSource;
    }

    public override IList<T> Query<T>(string sql) {
        var rowmapper = this.Context.GetRowMapper<T>();
        return this.QueryInternal(sql, rowmapper, Array.Empty<object>());
    }

    public override IList<T> Query<T>(string sql, RowMapper<T> mapper) 
        => this.QueryInternal(sql, mapper, Array.Empty<object>());

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

    public override T QueryForObject<T>(string sql) {
        throw new NotImplementedException();
    }

    public override T QueryForObject<T>(string sql, RowMapper<T> mapper) {
        throw new NotImplementedException();
    }

    public override int Update(string sql) => UpdateInternal(sql, Array.Empty<object>());

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
