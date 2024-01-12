namespace Autumn.Annotations;

/// <summary>
/// Specifies an alternative name for a parameter to be used when matching keys in a configuration source.
/// This attribute is intended to be used on parameters to indicate a different name than the parameter's actual name.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class ConfigNameAttribute : Attribute {

    /// <summary>
    /// Gets the alternative name specified for the parameter.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigNameAttribute"/> class with the specified alternative name.
    /// </summary>
    /// <param name="name">The alternative name to use for the parameter in configuration matching.</param>
    public ConfigNameAttribute(string name) {
        this.Name = name;
    }

}
