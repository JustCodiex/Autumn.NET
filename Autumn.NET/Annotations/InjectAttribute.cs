namespace Autumn.Annotations;

/// <summary>
/// Specifies that a property, constructor, method, or parameter should be injected with a dependency.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Parameter)]
public sealed class InjectAttribute : Attribute {

    /// <summary>
    /// Gets or sets the qualifier for the injected dependency.
    /// </summary>
    public string Qualifier { get; set; } = string.Empty;

}
