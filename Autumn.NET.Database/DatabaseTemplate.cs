using Autumn.Annotations.Library;
using Autumn.Database.Relational;

namespace Autumn.Database;

[AutumnTemplate]
public abstract class DatabaseTemplate {

    public abstract T QueryForObject<T>(string sql);

    public abstract T QueryForObject<T>(string sql, RowMapper<T> mapper);

    public abstract IList<T> Query<T>(string sql);

    public abstract IList<T> Query<T>(string sql, RowMapper<T> mapper);

    public abstract IList<T> Query<T>(string sql, RowMapper<T> mapper, params object?[] args);

    public abstract int Update(string sql);

    public abstract int Update(string sql, params object?[] args);

}
