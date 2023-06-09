namespace Autumn.Annotations.Library;

/// <summary>
/// Marks a class as an Autumn context loader, responsible for loading the application context.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class AutumnContextLoaderAttribute : Attribute {}
