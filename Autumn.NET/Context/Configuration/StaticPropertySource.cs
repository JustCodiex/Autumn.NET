using Autumn.Types;

namespace Autumn.Context.Configuration;

/// <summary>
/// Represents a static property-based configuration source.
/// </summary>
public class StaticPropertySource : IConfigSource {

    private readonly IDictionary<string, object?> values;

    /// <summary>
    /// Initializes a new instance of the <see cref="StaticPropertySource"/> class with the specified values.
    /// </summary>
    /// <param name="values">The dictionary of configuration values.</param>
    public StaticPropertySource(IDictionary<string, object?> values) { 
        this.values = values;
    }

    /// <inheritdoc/>
    public object? GetValue(string key) => values.TryGetValue(key, out object? obj) ? obj : throw new KeyNotFoundException();

    /// <inheritdoc/>
    public bool HasValue(string key) => values.ContainsKey(key);

    /// <summary>
    /// Gets the value from the configuration source for the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <param name="defaultValue">The default value to return if not found</param>
    /// <returns>The value associated with the key, or null if the key is not found.</returns>
    public T? GetValueOrDefault<T>(string key, T? defaultValue = default) {
        if (values.TryGetValue(key, out object? obj) && obj is not null) {
            return obj is T t ? t : (T)TypeConverter.Convert(obj, obj.GetType(), typeof(T))!;
        }
        return defaultValue;
    }

}
