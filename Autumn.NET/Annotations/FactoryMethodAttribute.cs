namespace Autumn.Annotations;

/// <summary>
/// Marks a method to be used as a factory method while constructing the declaring type.
/// </summary>
/// <remarks>
/// Multiple factory methods can be defined in a class (They must all be static) and the factory method to use is based on <see cref="InjectAttribute.FactoryArguments"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class FactoryMethodAttribute : Attribute {}
