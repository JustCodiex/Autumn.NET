namespace Autumn.Annotations;

/// <summary>
/// Specifies that a property should be populated with a value from a configuration source.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class ValueAttribute : Attribute {

    /// <summary>
    /// Gets the key used to retrieve the value from the configuration source.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets or sets the default value to be used if the configuration source does not provide a value.
    /// </summary>
    public string? Default { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueAttribute"/> class with the specified key.
    /// </summary>
    /// <param name="key">The key used to retrieve the value from the configuration source.</param>
    public ValueAttribute(string key) {
        this.Key = key;
    }

}
