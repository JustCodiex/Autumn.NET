namespace Autumn.Annotations;

/// <summary>
/// Specifies an alternative class to use when constructing the attributed class
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public sealed class FactoryAttribute(Type factoryClassType) : Attribute {

    /// <summary>
    /// Get the type of the factory class that can construct this attributed class
    /// </summary>
    public Type FactoryClass { get; } = factoryClassType;

    /// <summary>
    /// Get or set the name of the specific factory method to invoke
    /// </summary>
    public string? FactoryMethod { get; set; } = string.Empty;

}
