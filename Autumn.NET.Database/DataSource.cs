using Autumn.Annotations.Library;

namespace Autumn.Database;

[AutumnTemplate]
public abstract class DataSource {

    public abstract DatabaseCommand CreateCommand(string command);

}
