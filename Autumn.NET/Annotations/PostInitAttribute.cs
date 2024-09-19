namespace Autumn.Annotations;

/// <summary>
/// Specifies that a method should be executed after Autumn context initialization.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class PostInitAttribute : Attribute {
    
    /// <summary>
    /// Get or set if the post initialisation should execute within the context of an <see cref="Task"/>.
    /// </summary>
    public bool AsyncTask { get; set; } = false;

}
