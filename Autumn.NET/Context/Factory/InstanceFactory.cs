namespace Autumn.Context.Factory;

internal class InstanceFactory : IComponentFactory {

    private readonly ComponentIdentifier instanceIdentifier;
    private readonly AutumnAppContext context;

    internal InstanceFactory(AutumnAppContext context, Type type) {
        this.context = context;
        this.instanceIdentifier = ComponentIdentifier.DefaultIdentifier(type);
    }

    public void CreateFactory(ComponentIdentifier identifier) {
        return;
    }

    public object GetComponent(ComponentIdentifier identifier, object[] args, IScopeContext? scope) {
        if (identifier != instanceIdentifier) {
            throw new InvalidOperationException();
        }
        return context.CreateContextObject(identifier.ComponentInstanceType, null, args) ?? throw new Exception();
    }

}
