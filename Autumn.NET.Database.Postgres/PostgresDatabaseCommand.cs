using System.Text;

using Npgsql;

namespace Autumn.Database.Postgres;

public sealed class PostgresDatabaseCommand : DatabaseCommand {

    private readonly string commandText;
    private readonly NpgsqlConnection connection;
    private readonly NpgsqlCommand command;
    private readonly int arguments;

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

    public override void Dispose() {
        ((IDisposable)command).Dispose();
    }

    public override QueryResult Execute() {
        return new PostgresQueryResult(this.command.ExecuteReader());
    }

    public override int ExecuteUpdate() => this.command.ExecuteNonQuery();

    public override void SetArgument(int index, object? value) {
        if (index < 0) {
            throw new IndexOutOfRangeException("index");
        }
        if (index >= this.arguments) { throw new ArgumentOutOfRangeException("index"); }
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
            case DateOnly dateOnly:
                this.command.Parameters.Add(new NpgsqlParameter(argName, NpgsqlTypes.NpgsqlDbType.Timestamp) {
                    Value = dateOnly.ToDateTime(TimeOnly.MinValue)
                });
                break;
            case null:

                break;
            default:
                throw new NotSupportedException();
        }
    }

}
