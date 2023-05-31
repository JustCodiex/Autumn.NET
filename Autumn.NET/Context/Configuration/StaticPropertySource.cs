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

}
