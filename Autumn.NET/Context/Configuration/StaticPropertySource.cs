using Autumn.Reflection;
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

    /// <summary>
    /// Gets the value from the configuration source for the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the key, or null if the key is not found.</returns>
    public T? GetValue<T>(string key) {
        if (values.TryGetValue(key, out object? obj) && obj is not null) {
            return obj is T t ? t : (T)TypeConverter.Convert(obj, obj.GetType(), typeof(T))!;
        }
        IDictionary<string, object?> recordData = GetValues(key);
        if (recordData.Count > 0 && ComplexObjectBuilder.BuildObject(typeof(T), recordData) is T recordObject) {
            return recordObject;
        }
        throw new KeyNotFoundException();
    }

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
        IDictionary<string, object?> recordData = GetValues(key);
        if (recordData.Count > 0 && ComplexObjectBuilder.BuildObject(typeof(T), recordData) is T recordObject) {
            return recordObject;
        }
        return defaultValue;
    }

    /// <summary>
    /// Gets the key-values associated with the sub-path
    /// </summary>
    /// <param name="key">The key to extract dictionary from</param>
    /// <returns>The key-value pairs from the specified key</returns>
    public IDictionary<string, object?> GetValues(string key) => values.Where(x => x.Key.StartsWith(key + ".")).Select(x => (Key: x.Key[(key.Length + 1)..], x.Value)).ToDictionary(x => x.Key, x => x.Value);

}
