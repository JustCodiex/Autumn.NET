using System.Reflection;

namespace Autumn.Annotations;

/// <summary>
/// Attribute specifying the load/application order
/// </summary>
/// <remarks>
/// Currently only applicable for:
/// <list type="bullet">
///     <item>
///         <term><see cref="Http.Interceptors.AutumnHttpInterceptorChain"/></term>
///         <description>For specifying the order between intercepters. Has priority over the <see cref="Http.Annotations.InterceptBeforeAttribute"/> and the <see cref="Http.Annotations.InterceptAfterAttribute"/> attributes.</description>
///     </item>
/// </list>
/// </remarks>
/// <param name="order">The order value</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class OrderAttribute(int order) : Attribute {

    /// <summary>
    /// Get the order value
    /// </summary>
    public int Order { get; } = order;

    internal static int GetOrder(Type type) => type.GetCustomAttribute(typeof(OrderAttribute)) is OrderAttribute orderAttr ? orderAttr.Order : -1;

}
