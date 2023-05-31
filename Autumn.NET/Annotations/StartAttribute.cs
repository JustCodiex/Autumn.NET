namespace Autumn.Annotations;

/// <summary>
/// Specifies that a method should be invoked when the Autumn application starts.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class StartAttribute : Attribute {}
