namespace Autumn.Context.Factory;

internal class ScopeFactory : IComponentFactory {

    private readonly Dictionary<IScopeContext, object> scopeInstances;
    private readonly ComponentIdentifier instanceIdentifier;
    private readonly AutumnAppContext context;

    internal ScopeFactory(AutumnAppContext context, Type type) {
        this.context = context;
        this.instanceIdentifier = ComponentIdentifier.DefaultIdentifier(type);
        this.scopeInstances = new();
    }

    public void CreateFactory(ComponentIdentifier identifier) {
        return;
    }

    public object GetComponent(ComponentIdentifier identifier, object[] args, IScopeContext? scope) {
        if (identifier != instanceIdentifier) {
            throw new InvalidOperationException();
        }
        if (scope is null) {
            throw new InvalidOperationException("Cannot Create scoped object without a scope");
        }
        if (scopeInstances.TryGetValue(scope, out var instance)) {
            return instance;
        }
        object scopedInstance = context.CreateContextObject(identifier.ComponentInstanceType, null, args, scope) ?? throw new Exception();
        CreateScopeInstance(scope, scopedInstance);
        return scopedInstance;
    }

    private void CreateScopeInstance(IScopeContext context, object scopeInstance) {
        scopeInstances[context] = scopeInstance;
        context.OnContextDestroyed += this.HandleScopeDestruction;
    }

    private void HandleScopeDestruction(IScopeContext scopeContext) { // Remove reference from dictionary => allow GC to cleanup objects
        scopeInstances.Remove(scopeContext);
    }

}
