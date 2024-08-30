namespace Autumn.Http.Annotations;

/// <summary>
/// Specifies the attributed class should run its interceptor logic before the specified interceptor.
/// </summary>
/// <remarks>
/// This attribute takes precedence over <see cref="InterceptAfterAttribute"/>.
/// </remarks>
/// <param name="interceptBefore">The interceptor to run own logic before</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class InterceptBeforeAttribute(Type interceptBefore) : Attribute {

    /// <summary>
    /// Get the type to intercept HTTP requests before.
    /// </summary>
    public Type InterceptBefore { get; } = interceptBefore;

}
