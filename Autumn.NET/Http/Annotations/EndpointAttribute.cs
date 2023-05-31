namespace Autumn.Http.Annotations;

/// <summary>
/// Specifies that a method represents an HTTP endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class EndpointAttribute : Attribute {

    /// <summary>
    /// Gets the endpoint URL associated with the method.
    /// </summary>
    public string Endpoint { get; }

    /// <summary>
    /// Gets or sets the HTTP method for the endpoint. Default is "GET".
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// Initializes a new instance of the <see cref="EndpointAttribute"/> class with the specified endpoint URL.
    /// </summary>
    /// <param name="endpoint">The endpoint URL.</param>
    public EndpointAttribute(string endpoint) { 
        this.Endpoint = endpoint;
    }

}
