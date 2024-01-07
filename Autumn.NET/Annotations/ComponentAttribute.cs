namespace Autumn.Annotations;

/// <summary>
/// Specifies the scope of a component within the Autumn framework.
/// </summary>
public enum ComponentScope {

    /// <summary>
    /// Indicates that the component should be treated as a singleton, resulting in a single instance shared across the application.
    /// </summary>
    Singleton,

    /// <summary>
    /// Indicates that the component should be treated as a multiton, resulting in multiple instances created based on usage or context.
    /// </summary>
    Multiton,

    /// <summary>
    /// Indicates that the component should be treated as a context-sensitive multiton, resulting in multiple instances are created and shared within a given context.
    /// </summary>
    Scoped,

}

/// <summary>
/// Specifies that a class is a component within the Autumn framework.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ComponentAttribute : Attribute {

    /// <summary>
    /// Gets or sets the scope of the component.
    /// </summary>
    /// <remarks>
    /// Default value is <see cref="ComponentScope.Singleton"/>.
    /// </remarks>
    public ComponentScope Scope { get; set; } = ComponentScope.Singleton;

}
