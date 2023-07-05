using System.Reflection;

using Autumn.Annotations.Library;
using Autumn.Context;
using Autumn.Database.Annotations;
using Autumn.Database.Relational;

namespace Autumn.Database.Lib;

/// <summary>
/// The database-specific implementation of the application context loader in the Autumn framework.
/// </summary>
[AutumnContextLoader]
public class DatabaseApplicationContext {

    private readonly Dictionary<Type, GeneratedRowMapper> rowmappers;

    public DatabaseApplicationContext() { 
        this.rowmappers = new Dictionary<Type, GeneratedRowMapper>();
    }

    /// <summary>
    /// Retrieves the row mapper for the specified type from the application context.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <returns>The row mapper function.</returns>
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

    /// <summary>
    /// Loads the database-related components into the application context.
    /// </summary>
    /// <param name="context">The AutumnAppContext instance.</param>
    /// <param name="types">The types to process for database-related components.</param>
    public static void LoadContext(AutumnAppContext context, IList<Type> types) { // This method is called automatically because of the AutumnContextLoader attribute

        DatabaseApplicationContext databaseLibraryContext = new DatabaseApplicationContext();

        for (int i = 0; i < types.Count; i++) {

            if (types[i].GetCustomAttribute<RowAttribute>() is RowAttribute rowAttribute) {
                var mapper = CreateRowMapper(types[i], rowAttribute);
                databaseLibraryContext.rowmappers[types[i]] = mapper;
            }

        }

        // Register the database context as a component in the application context
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
            return instance!;
        };
    }

}
