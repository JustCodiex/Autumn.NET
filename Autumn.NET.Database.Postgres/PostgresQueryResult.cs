using Npgsql;

namespace Autumn.Database.Postgres;

public sealed class PostgresQueryResult : QueryResult {

    private readonly NpgsqlDataReader reader;

    public PostgresQueryResult(NpgsqlDataReader dataReader) { 
        this.reader = dataReader;
    }

    public override void Dispose() {
        reader.Dispose();
    }

    public override int GetColumnIndex(string name) 
        => reader.GetOrdinal(name);

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

    public override bool Read() => reader.Read();

}
