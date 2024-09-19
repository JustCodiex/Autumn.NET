using System.Reflection;

namespace Autumn.Proxying;

/// <summary>
/// Interface for interacting with a proxy.
/// </summary>
public interface IProxy {

    /// <summary>
    /// Handles a method call made through the <see cref="IProxy"/> instance.
    /// </summary>
    /// <param name="targetMethod">The method that was invoked.</param>
    /// <param name="arguments">The arguments that were given at time of invocation.</param>
    /// <returns>The result of the proxy operation.</returns>
    object? HandleMethod(MethodInfo targetMethod, object?[] arguments);

}
