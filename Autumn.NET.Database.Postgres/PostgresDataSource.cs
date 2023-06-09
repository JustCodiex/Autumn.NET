using Autumn.Annotations;
using Autumn.Annotations.Library;
using Autumn.Context.Configuration;

using Npgsql;

namespace Autumn.Database.Postgres;

/// <summary>
/// Represents a PostgreSQL data source implementation of <see cref="DataSource"/>.
/// </summary>
[AutumnTemplateImplementation(typeof(DataSource))]
public class PostgresDataSource : DataSource {

    private readonly NpgsqlConnection connection;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostgresDataSource"/> class.
    /// </summary>
    /// <param name="propertySource">The property source for retrieving connection details.</param>
    public PostgresDataSource([Inject] StaticPropertySource propertySource) {

        string connectionurl = (string)propertySource.GetValue("autumn.datasource.url");
        int connectionPort = (int)propertySource.GetValue("autumn.datasource.port");
        string database = (string)propertySource.GetValue("autumn.datasource.database");
        string username = (string)propertySource.GetValue("autumn.datasource.username");
        string password = (string)propertySource.GetValue("autumn.datasource.password");

        string connectionString = $"Server={connectionurl};Port={connectionPort};Database={database};User Id={username};Password={password};";
        connection = new NpgsqlConnection(connectionString);
        try {
            connection.Open();
        } catch (Exception e) {
            throw;
        }

    }

    /// <inheritdoc />
    public override DatabaseCommand CreateCommand(string commandText) {
        return new PostgresDatabaseCommand(commandText, connection);
    }

    // TODO: Shutdown hook

}
