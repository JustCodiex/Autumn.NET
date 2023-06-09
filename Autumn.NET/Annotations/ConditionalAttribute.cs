namespace Autumn.Annotations;

/// <summary>
/// Represents a conditional attribute that can be applied to properties or classes specifying if the component/service/configuration should be included in the component scan.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class ConditionalAttribute : Attribute {

    /// <summary>
    /// Gets or sets the value that the property should have for the condition to be met.
    /// </summary>
    public string? HasValue { get; set; }

    /// <summary>
    /// Gets the name of the property used as the condition.
    /// </summary>
    public string Property { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalAttribute"/> class
    /// with the specified property name.
    /// </summary>
    /// <param name="property">The name of the property used as the condition.</param>
    public ConditionalAttribute(string property) { 
        this.Property = property;
    }

}
