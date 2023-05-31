namespace Autumn.Annotations;

/// <summary>
/// Specifies that a method should be executed after Autumn context initialization.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class PostInitAttribute : Attribute { }
