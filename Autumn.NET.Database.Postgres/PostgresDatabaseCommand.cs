using System.Text;

using Npgsql;

namespace Autumn.Database.Postgres;

/// <summary>
/// Represents a PostgreSQL implementation of the <see cref="DatabaseCommand"/> class.
/// </summary>
public sealed class PostgresDatabaseCommand : DatabaseCommand {

    private readonly string commandText;
    private readonly NpgsqlConnection connection;
    private readonly NpgsqlCommand command;
    private readonly int arguments;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresDatabaseCommand"/> class with the specified command text and PostgreSQL connection.
    /// </summary>
    /// <param name="commandText">The command text.</param>
    /// <param name="connection">The PostgreSQL connection.</param>
    public PostgresDatabaseCommand(string commandText, NpgsqlConnection connection) { 
        this.connection = connection;
        (this.commandText, this.arguments) = GetCommandText(commandText);
        this.command = new NpgsqlCommand(this.commandText, connection);
    }

    private static (string, int) GetCommandText(string commandText) {
        int nextArgument = commandText.IndexOf('?');
        if (nextArgument == -1) {
            return (commandText, 0);
        }
        int count = 0;
        int lastIndex = 0;
        StringBuilder sb = new StringBuilder();
        while (nextArgument != -1) {
            string subsequence = commandText[lastIndex.. nextArgument];
            sb.Append(subsequence);
            sb.Append('@');
            sb.Append("arg");
            sb.Append(count++);
            lastIndex = nextArgument+1;
            nextArgument = commandText.IndexOf('?', nextArgument + 1);
        }
        sb.Append(commandText[lastIndex..]);
        return (sb.ToString(), count);
    }

    /// <inheritdoc/>
    public override void Dispose() {
        ((IDisposable)command).Dispose();
    }

    /// <inheritdoc/>
    public override QueryResult Execute() {
        return new PostgresQueryResult(this.command.ExecuteReader());
    }

    /// <inheritdoc/>
    public override int ExecuteUpdate() => this.command.ExecuteNonQuery();

    /// <inheritdoc/>
    public override void SetArgument(int index, object? value) { // TODO: Try determine db type from type hints (ie. from the Column attribute)
        if (index < 0) {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        if (index >= this.arguments) { throw new ArgumentOutOfRangeException(nameof(index)); }
        string argName = $"@arg{index}";
        switch (value) {
            case string str:
                this.command.Parameters.Add(new NpgsqlParameter(argName, NpgsqlTypes.NpgsqlDbType.Varchar) {
                    Value = str
                });
                break;
            case decimal dec:
                this.command.Parameters.Add(new NpgsqlParameter(argName, NpgsqlTypes.NpgsqlDbType.Numeric) {
                    Value = dec
                });
                break;
            case int i32:
                this.command.Parameters.Add(new NpgsqlParameter(argName, NpgsqlTypes.NpgsqlDbType.Integer) {
                    Value = i32
                });
                break;
            case long i64:
                this.command.Parameters.Add(new NpgsqlParameter(argName, NpgsqlTypes.NpgsqlDbType.Bigint) {
                    Value = i64
                });
                break;
            case DateOnly dateOnly:
                this.command.Parameters.Add(new NpgsqlParameter(argName, NpgsqlTypes.NpgsqlDbType.Timestamp) {
                    Value = dateOnly.ToDateTime(TimeOnly.MinValue)
                });
                break;
            case DateTime dateOnly:
                this.command.Parameters.Add(new NpgsqlParameter(argName, NpgsqlTypes.NpgsqlDbType.Timestamp) {
                    Value = dateOnly
                });
                break;
            case bool bol:
                this.command.Parameters.Add(new NpgsqlParameter(argName, NpgsqlTypes.NpgsqlDbType.Boolean) {
                    Value = bol
                });
                break;
            case Guid guid:
                this.command.Parameters.Add(new NpgsqlParameter(argName, NpgsqlTypes.NpgsqlDbType.Uuid) {
                    Value = guid
                });
                break;
            case byte[] bytes:
                this.command.Parameters.Add(new NpgsqlParameter(argName, NpgsqlTypes.NpgsqlDbType.Bytea) {
                    Value = bytes
                });
                break;
            case null:
                throw new NotImplementedException();
            default:
                throw new NotSupportedException($"Unsupported type: {value.GetType()}");
        }
    }

}
