namespace Autumn.Annotations;

/// <summary>
/// Specifies that a class represents a configuration component.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ConfigurationAttribute : Attribute {

    /// <summary>
    /// Gets or sets the source of the configuration.
    /// </summary>
    public string Source { get; set; } = string.Empty;

}
