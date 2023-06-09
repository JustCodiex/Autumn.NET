namespace Autumn.Annotations.Library;

/// <summary>
/// Marks a class as an Autumn template, which provides a common contract between an abstract definition 
/// and a concrete implementation through the use of <see cref="AutumnTemplateImplementationAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AutumnTemplateAttribute : Attribute {}
