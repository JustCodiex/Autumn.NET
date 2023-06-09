namespace Autumn.Annotations.Library;

/// <summary>
/// Marks a class as an implementation of an Autumn template, providing a specific implementation
/// for the template specified by the <see cref="TemplateType"/>.
/// </summary>
public sealed class AutumnTemplateImplementationAttribute : ComponentAttribute {

    /// <summary>
    /// Gets the type of the Autumn template that this class implements.
    /// </summary>
    public Type TemplateType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AutumnTemplateImplementationAttribute"/> class
    /// with the specified Autumn template type.
    /// </summary>
    /// <param name="templateType">The type of the Autumn template that this class implements.</param>
    public AutumnTemplateImplementationAttribute(Type templateType) {
        this.TemplateType = templateType;
    }

}
