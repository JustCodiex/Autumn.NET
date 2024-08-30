namespace Autumn.Context.Configuration;

/// <summary>
/// Represents a configuration source.
/// </summary>
public interface IConfigSource {

    /// <summary>
    /// Checks if the configuration source contains a value for the specified key.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <returns>True if the configuration source contains a value for the key; otherwise, false.</returns>
    bool HasValue(string key);

    /// <summary>
    /// Gets the value from the configuration source for the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <returns>The value associated with the key, or null if the key is not found.</returns>
    object? GetValue(string key);

    /// <summary>
    /// Gets the value from the configuration source for the specified key.
    /// </summary>
    /// <param name="key">The key of the value to retrieve.</param>
    /// <param name="defaultValue">The default value to return if not found</param>
    /// <returns>The value associated with the key, or null if the key is not found.</returns>
    T? GetValueOrDefault<T>(string key, T? defaultValue = default);

}
