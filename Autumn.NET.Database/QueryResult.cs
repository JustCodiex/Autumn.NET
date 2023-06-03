namespace Autumn.Database;

public abstract class QueryResult : IDisposable {

    public abstract bool Read();

    public abstract void Dispose();

    public abstract int GetColumnIndex(string name);

    public abstract object? GetColumnValue(Type type, int columnIndex);

}
