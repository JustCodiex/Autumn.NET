namespace Autumn.Annotations;

/// <summary>
/// Specifies that a class represents a service.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ServiceAttribute : Attribute {

    /// <summary>
    /// Gets or sets the qualifier for the service.
    /// </summary>
    public string Qualifier { get; set; } = string.Empty;

}
