namespace Autumn.Context.Factory;

internal class SingletonFactory : IComponentFactory {

    private readonly Dictionary<ComponentIdentifier, object> componentInstances;
    private readonly AutumnAppContext appContext;

    public SingletonFactory(AutumnAppContext context) {
        this.appContext = context;
        this.componentInstances = new Dictionary<ComponentIdentifier, object>();
    }

    public void CreateFactory(ComponentIdentifier identifier) {
        return;
    }

    public object GetComponent(ComponentIdentifier identifier) {
        if (componentInstances.TryGetValue(identifier, out object? singleton)) {
            return singleton;
        }
        var component = appContext.CreateContextObject(identifier.ComponentInstanceType) ?? throw new Exception();
        componentInstances.Add(identifier, component);
        return component;
    }

    public bool HasSingleton(ComponentIdentifier componentIdentifier) {
        return componentInstances.ContainsKey(componentIdentifier);
    }

    public void RegisterSingleton(ComponentIdentifier identifier, object singleton) {
        componentInstances[identifier] = singleton;
    }

}
