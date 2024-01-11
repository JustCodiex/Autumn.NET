namespace Autumn.Http.Annotations;

/// <summary>
/// Attribute for designating a method as the exception handler for specific exception types
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class EndpointExceptionHandlerAttribute : Attribute {

    /// <summary>
    /// The exception type to handle
    /// </summary>
    public Type ExceptionType { get; }

    /// <summary>
    /// Initialises a new <see cref="EndpointExceptionHandlerAttribute"/> instance for the specified exception.
    /// </summary>
    /// <param name="exceptionType">The exception type to handle</param>
    public EndpointExceptionHandlerAttribute(Type exceptionType) {
        this.ExceptionType = exceptionType;
    }

}
