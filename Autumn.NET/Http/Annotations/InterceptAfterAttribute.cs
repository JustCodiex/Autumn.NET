namespace Autumn.Http.Annotations;

/// <summary>
/// Specifies the attributed class should run its interceptor logic after the specified interceptor.
/// </summary>
/// <param name="interceptAfter">The interceptor to run own logic after</param>
/// <remarks>
/// The <see cref="InterceptBeforeAttribute"/> attribute has precedence over this.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class InterceptAfterAttribute(Type interceptAfter) : Attribute {

    /// <summary>
    /// Get the type to intercept HTTP requests after.
    /// </summary>
    public Type InterceptAfter { get; } = interceptAfter;

}
