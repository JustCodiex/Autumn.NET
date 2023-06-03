using System.Reflection;

using Autumn.Annotations.Library;
using Autumn.Context;
using Autumn.Database.Annotations;
using Autumn.Database.Relational;

namespace Autumn.Database.Lib;

[AutumnContextLoader]
public class DatabaseLibraryContext {

    private readonly Dictionary<Type, GeneratedRowMapper> rowmappers;

    public DatabaseLibraryContext() { 
        this.rowmappers = new Dictionary<Type, GeneratedRowMapper>();
    }

    public RowMapper<T> GetRowMapper<T>() {
        if (rowmappers.TryGetValue(typeof(T), out var rowMapper)) {
            return (QueryResult r, int i) => {
                if (rowMapper(r,i) is T t) {
                    return t;
                }
                throw new InvalidOperationException();
            };
        }
        throw new KeyNotFoundException();
    }

    public static void LoadContext(AutumnAppContext context, Type[] types) {

        DatabaseLibraryContext databaseLibraryContext = new DatabaseLibraryContext();

        for (int i = 0; i < types.Length; i++) {

            if (types[i].GetCustomAttribute<RowAttribute>() is RowAttribute rowAttribute) {
                var mapper = CreateRowMapper(types[i], rowAttribute);
                databaseLibraryContext.rowmappers[types[i]] = mapper;
            }

        }

        // Register
        context.RegisterComponent(databaseLibraryContext);

    }

    private static GeneratedRowMapper CreateRowMapper(Type target, RowAttribute rowAttribute) { // TODO: Generate using CIL
        var columns = target.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => (x, x.GetCustomAttribute<ColumnAttribute>()))
            .Where(x => x.Item2 is not null)
            .ToArray();
        return (QueryResult result, int index) => {
            var instance = Activator.CreateInstance(target);
            foreach (var column in columns) {
                int ordinal = result.GetColumnIndex(string.IsNullOrEmpty(column.Item2.ColumnName) ? column.x.Name : column.Item2.ColumnName);
                column.x.SetValue(instance, result.GetColumnValue(column.x.PropertyType, ordinal));
            }
            return instance;
        };
    }

}
