namespace Autumn.Database;

public abstract class DatabaseCommand : IDisposable {

    public abstract QueryResult Execute();

    public abstract int ExecuteUpdate();

    public abstract void SetArgument(int index, object? value);

    public abstract void Dispose();

}
