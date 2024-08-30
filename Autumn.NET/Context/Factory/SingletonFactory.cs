namespace Autumn.Context.Factory;

internal class SingletonFactory : IComponentFactory {

    private readonly Dictionary<ComponentIdentifier, object> componentInstances;
    private readonly AutumnAppContext appContext;

    public SingletonFactory(AutumnAppContext context) {
        this.appContext = context;
        this.componentInstances = new Dictionary<ComponentIdentifier, object>() {
            [new ComponentIdentifier(typeof(AutumnAppContext).FullName!, typeof(AutumnAppContext))] = context
        };
    }

    public void CreateFactory(ComponentIdentifier identifier) {
        return;
    }

    public object GetComponent(ComponentIdentifier identifier, object[] args, IScopeContext? scope) {
        if (componentInstances.TryGetValue(identifier, out object? singleton)) {
            return singleton;
        }
        return appContext.CreateContextObject(identifier.ComponentInstanceType, x => componentInstances.Add(identifier, x), args) ?? throw new Exception();
    }

    public bool HasSingleton(ComponentIdentifier componentIdentifier) {
        return componentInstances.ContainsKey(componentIdentifier);
    }

    public void RegisterSingleton(ComponentIdentifier identifier, object singleton) {
        componentInstances[identifier] = singleton;
        Type[] interfaces = identifier.ComponentInstanceType.GetInterfaces();
        for (int i = 0; i <  interfaces.Length; i++) {
            var interfaceIdentifier = ComponentIdentifier.DefaultIdentifier(interfaces[i]);
            if (!HasSingleton(interfaceIdentifier)) {
                componentInstances[interfaceIdentifier] = singleton;
            }       
        }
    }

}
