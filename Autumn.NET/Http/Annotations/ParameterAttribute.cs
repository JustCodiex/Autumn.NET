namespace Autumn.Http.Annotations;

/// <summary>
/// Specifies that a method parameter should be interpreted as coming from a URL query.
/// This class cannot be inherited.
/// </summary>
/// <remarks>
/// Use this attribute to bind method parameters to specific URL query parameters.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
public class ParameterAttribute : Attribute {

    /// <summary>
    /// Gets the name of the parameter.
    /// </summary>
    /// <value>The name of the parameter as expected in the URL query.</value>
    public string Name { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterAttribute"/> class with the specified parameter name.
    /// </summary>
    /// <param name="name">The name of the parameter as it should be recognized in the URL query.</param>
    /// <remarks>
    /// The name corresponds to the key in the query string, e.g., in "example.com?param=value", the name should be "param".
    /// </remarks>
    public ParameterAttribute(string name) {
        this.Name = name;
    }

}
