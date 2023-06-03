namespace Autumn.Annotations;

/// <summary>
/// Specifies that a method is an entry point for the Autumn application.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class EntryPointAttribute : Attribute {}
