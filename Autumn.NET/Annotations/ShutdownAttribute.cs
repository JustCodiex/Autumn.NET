namespace Autumn.Annotations;

/// <summary>
/// Marks a method as a graceful shutdown handler
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ShutdownAttribute : Attribute {

    /// <summary>
    /// Marks whether the method should only be invoked on graceful shutdown events
    /// </summary>
    public bool GracefulShutdownOnly { get; set; }

}
