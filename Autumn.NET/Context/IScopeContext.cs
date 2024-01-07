namespace Autumn.Context;

/// <summary>
/// Represents a delegate for handling the event when a ScopeContext is destroyed.
/// </summary>
/// <param name="scopeContext">The instance of the <see cref="IScopeContext"/> that is being destroyed.</param>
public delegate void ScopeContextDestroyedHandler(IScopeContext scopeContext);

/// <summary>
/// Defines the interface for a scope context that supports destruction and notification upon destruction.
/// </summary>
public interface IScopeContext {

    /// <summary>
    /// Occurs when the context is destroyed. 
    /// </summary>
    event ScopeContextDestroyedHandler? OnContextDestroyed;

    /// <summary>
    /// Destroys the current scope context, cleaning up any resources.
    /// </summary>
    void Destroy();

}
