using System.Reflection;

using Autumn.Annotations;
using Autumn.Context.Configuration;
using Autumn.Context.Factory;

namespace Autumn.Context;

/// <summary>
/// Represents the application context for the Autumn framework.
/// It provides dependency injection and configuration management functionality.
/// </summary>
public sealed class AutumnAppContext {

    private record struct TypeCreator(Type Type, IComponentFactory Factory);

    private readonly SingletonFactory singletonFactory;
    private readonly Dictionary<string, HashSet<TypeCreator>> services;
    private readonly Dictionary<string, HashSet<TypeCreator>> components;
    private readonly Dictionary<Type, object> configurationInstances;
    private readonly HashSet<IConfigSource> configSources;
    private readonly HashSet<Type> interceptedTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutumnAppContext"/> class.
    /// </summary>
    public AutumnAppContext() {
        singletonFactory = new SingletonFactory(this);
        services = new();
        configurationInstances = new();
        configSources = new();
        components = new();
        interceptedTypes = new();
    }

    /// <summary>
    /// Registers a service type to be resolved by the application context.
    /// The service type will be resolved using its name as the qualifier.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    public void RegisterService<T>() => RegisterService(typeof(T));

    /// <summary>
    /// Registers a service type with a specific qualifier to be resolved by the application context.
    /// </summary>
    /// <typeparam name="T">The type of the service.</typeparam>
    /// <param name="qualifier">The qualifier to identify the service.</param>
    public void RegisterService<T>(string qualifier) => RegisterService(typeof(T), qualifier);

    /// <summary>
    /// Registers a service type to be resolved by the application context.
    /// The service type will be resolved using its name as the qualifier.
    /// </summary>
    /// <param name="serviceType">The type of the service.</param>
    public void RegisterService(Type serviceType) => RegisterService(serviceType, serviceType.Name);

    /// <summary>
    /// Registers a service type with a specific qualifier to be resolved by the application context.
    /// </summary>
    /// <param name="serviceType">The type of the service.</param>
    /// <param name="qualifier">The qualifier to identify the service.</param>
    public void RegisterService(Type serviceType, string qualifier) {
        var creator = new TypeCreator(serviceType, singletonFactory);
        RegisterComponentInternal(serviceType, creator);
        if (services.ContainsKey(qualifier)) {
            services[qualifier].Add(creator);
            return;
        }
        services.Add(qualifier, new() { creator });
    }

    public void RegisterComponent(Type component) {
        var factory = GetComponentFactory(component);
        TypeCreator creator = new TypeCreator(component, factory);
        RegisterComponentInternal(component, creator);
    }

    private void RegisterComponentInternal(Type component, TypeCreator creator) {
        AssociateComponent(ComponentIdentifier.DefaultIdentifier(component), creator);
        var interfaces = component.GetInterfaces();
        foreach (var type in interfaces) {
            AssociateComponent(new ComponentIdentifier(type.FullName!, component), creator);
        }
        var baseType = component.BaseType;
        if (baseType is not null && baseType != typeof(object)) {
            AssociateComponent(new ComponentIdentifier(baseType.FullName!, component), creator);
        }
    }

    private void AssociateComponent(ComponentIdentifier identifier, TypeCreator creator) {
        if (components.TryGetValue(identifier.ComponentQualifier, out var keyTypes)) {
            keyTypes.Add(creator);
            return;
        }
        components.Add(identifier.ComponentQualifier, new () { creator });
    }

    public void RegisterComponent<T>(T component) where T : notnull {
        var t = typeof(T);
        var componentType = t.IsInterface || t.IsAbstract ? component.GetType() : t;
        RegisterComponent(componentType);
        var identifier = ComponentIdentifier.DefaultIdentifier(componentType);
        singletonFactory.RegisterSingleton(identifier, component);
    }

    /// <summary>
    /// Retrieves an array of all registered service types.
    /// </summary>
    /// <returns>An array of registered service types.</returns>
    internal Type[] GetServices() {
        HashSet<Type> services = new HashSet<Type>();
        foreach (var (_, creators) in this.services) {
            foreach (var creator in creators) {
                services.Add(creator.Type);
            }
        }
        return services.ToArray();
    }

    /// <summary>
    /// Registers a configuration instance for a specific type.
    /// </summary>
    /// <param name="type">The type of the configuration.</param>
    /// <param name="configuration">The configuration instance.</param>
    public void RegisterConfiguration(Type type, object configuration) {
        ApplyValueAttribute(type, configuration, configSources);
        configurationInstances[type] = configuration;
        singletonFactory.RegisterSingleton(ComponentIdentifier.DefaultIdentifier(type), configuration);
    }

    /// <summary>
    /// Registers configuration sources to be used for retrieving configuration values.
    /// </summary>
    /// <param name="sources">The configuration sources.</param>
    public void RegisterConfigSource(params IConfigSource[] sources) {
        for (int i = 0; i < sources.Length; i++) {
            configSources.Add(sources[i]);
        }
    }

    /// <summary>
    /// Retrieves an instance of the specified type from the application context.
    /// </summary>
    /// <param name="type">The type of the instance to retrieve.</param>
    /// <returns>The instance of the specified type, if found; otherwise, null.</returns>
    public object? GetInstanceOf(Type type) {

        var key = ComponentIdentifier.DefaultIdentifier(type);
        if (components.TryGetValue(key.ComponentQualifier, out var instance) && instance.FirstOrDefault(x => x.Type == type) is TypeCreator creator) {
            return creator.Factory.GetComponent(key);
        }

        if (services.TryGetValue(type.FullName!, out var instances) && instances.Any(x => x.Type == type)) {
            //return CreateService(type);
        }

        return null;

    }

    internal object? CreateContextObject(Type componentType) {

        // Get best constructor
        var (ctor, callargs) = GetConstructor(componentType);
        if (ctor is null) {
            return null;
        }

        // Create
        var component = ctor.Invoke(callargs) ?? throw new Exception();

        // Initialise
        InitialiseContextObject(component, componentType);

        // Return the component
        return component;

    }

    internal void InitialiseContextObject(object component, Type componentType) {

        // Setup values
        ApplyValueAttribute(componentType, component, configSources);

        // Inject dependencies
        InjectDependencies(component, componentType);

        // Trigger post init
        RunPostInit(component, componentType);

    }

    private (ConstructorInfo?, object?[] callargs) GetConstructor(Type klass, params object[] args) {

        // Get constructors
        var ctors = klass.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var ctor in ctors) {
            var ctorArgs = ctor.GetParameters();
            if (ctor.GetCustomAttribute<InjectAttribute>() is not null) {
                throw new NotImplementedException();
            } else {
                var attributedArgs = ctorArgs.Select(x => (x, x.GetCustomAttribute<InjectAttribute>())).ToArray();
                var injectArgs = attributedArgs.Where(x => x.Item2 is not null).ToArray();
                var passArgs = attributedArgs.Where(x => x.Item2 is null).ToArray();
                if (passArgs.Length == args.Length && passArgs.Zip(args).All(x => x.First.x == x.Second)) {
                    var callArgs = new object?[ctorArgs.Length];
                    int j = 0;
                    for (int i = 0; i < callArgs.Length; i++) {
                        if (attributedArgs[i].Item2 is InjectAttribute inject) {
                            callArgs[i] = SolveInjectDependency(attributedArgs[i].x.ParameterType, attributedArgs[i].x.Name!, inject);
                        } else {
                            callArgs[i] = passArgs[j++];
                        }
                    }
                    return (ctor, callArgs);
                }
            }
        }

        return (null, Array.Empty<object>());

    }

    private object? GetComponent((string,Type) componentIdentifier) {
        var identifier = new ComponentIdentifier(componentIdentifier.Item1, componentIdentifier.Item2);
        if (singletonFactory.HasSingleton(identifier)) {
            return singletonFactory.GetComponent(identifier);
        }
        if (components.TryGetValue(componentIdentifier.Item1, out var typesByQualifier)) {
            throw new NotImplementedException();
        }
        if (components.TryGetValue(componentIdentifier.Item2.FullName!, out var typesByType)) {
            var qualifiedByName = typesByType.Where(x => x.Type.Name == componentIdentifier.Item1).ToList();
            if (qualifiedByName.Count == 1) {
                return qualifiedByName[0].Factory.GetComponent(ComponentIdentifier.DefaultIdentifier(qualifiedByName[0].Type));
            }
            if (typesByType.Count == 1) {
                return typesByType.First().Factory.GetComponent(ComponentIdentifier.DefaultIdentifier(typesByType.First().Type));
            }
            throw new NotImplementedException();
        }
        return null; // TODO: Chechk if null
    }

    private void ApplyValueAttribute(Type klass, object instance, ICollection<IConfigSource> sources) {

        var properties = klass.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(x => (x, x.GetCustomAttribute<ValueAttribute>()))
                .Where(x => x.Item2 is not null);

        foreach (var (propertyInfo, propertyDesc) in properties) {
            if (!LookupConfigValue(propertyInfo.PropertyType, sources, propertyDesc!.Key, propertyDesc.Default, out object? value)) {
                continue;
            }
            SetValue(instance, propertyInfo, value);
        }

    }

    private bool LookupConfigValue(Type requestedType, ICollection<IConfigSource> sources, string key, string? defaultValue, out object? value) {

        value = null;
        foreach (var source in sources) {
            if (source.HasValue(key)) {
                value = source.GetValue(key);
                if (value is not null && value.GetType() != requestedType) {
                    value = Convert.ChangeType(value, requestedType);
                }
                return true;
            }
        }

        if (!string.IsNullOrEmpty(defaultValue) && Convert.ChangeType(defaultValue, requestedType) is object someValue) {
            value = someValue;
            return true;
        }

        return false;

    }

    private void InjectDependencies(object target, Type targetType) {

        // Get properties
        var injectProperties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(x => (x, x.GetCustomAttribute<InjectAttribute>())).Where(x => x.Item2 is not null);

        // Loop over all properties
        foreach (var property in injectProperties) {

            // Get inject value
            object? injectValue = SolveInjectDependency(property.x.PropertyType, property.x.Name, property.Item2!);

            // Set it
            SetValue(target, property.x, injectValue);

        }

    }

    private object? SolveInjectDependency(Type expectedType, string implicitQualifier, InjectAttribute inject) {
        if (!string.IsNullOrEmpty(inject.Qualifier)) {
            throw new NotImplementedException();
        }
        var byImplicitQualifier = (implicitQualifier, expectedType);
        if (!string.IsNullOrEmpty(implicitQualifier) && GetComponent(byImplicitQualifier) is object component) {
            return component;
        }
        if (GetComponent((expectedType.FullName!, expectedType)) is object compoennt2) {
            return compoennt2;
        }
        return null; // TODO: Check if null
    }

    private void SetValue(object instance, PropertyInfo propertyInfo, object? value) {
        var klass = propertyInfo.DeclaringType!;
        if (propertyInfo.SetMethod is MethodInfo setter) {
            setter.Invoke(instance, new[] { value });
        } else {
            var fields = klass.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            if (fields.FirstOrDefault(x => x.Name == $"<{propertyInfo.Name}>k__BackingField") is FieldInfo field) {
                field.SetValue(instance, value);
            }
        }
    }

    private void RunPostInit(object target, Type targetType) {
        var postInits = targetType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(x => (x, x.GetCustomAttribute<PostInitAttribute>()))
                .Where(x => x.Item2 is not null);
        foreach (var (post, _) in postInits) {
            post.Invoke(target, Array.Empty<object>());
        }
    }

    private void HandleMethodInterceptors(Type targetType) {
        var methods = targetType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var postprocs = methods.Select(x => (x, x.GetCustomAttributes<PostProcessorAttribute>().ToList())).Where(x => x.Item2.Count > 0).ToArray();
        var exceptionHandlers = methods.Select(x => (x, x.GetCustomAttribute<ExceptionHandlerAttribute>())).Where(x => x.Item2 is not null).ToArray();
        if ((postprocs.Length == 0 && exceptionHandlers.Length == 0) || interceptedTypes.Contains(targetType)) {
            return;
        }

        var targetMethods = postprocs.Select(x => x.x).Union(exceptionHandlers.Select(x => x.x)).ToArray();
        for (int i = 0; i < targetMethods.Length; i++) {

            var targetPostProcessor = postprocs.Where(x => x.x == targetMethods[i]).Select(x => x.Item2).FirstOrDefault() ?? new List<PostProcessorAttribute>();
            var targetException = exceptionHandlers.Where(x => x.x == targetMethods[i]).Select(x => x.Item2).FirstOrDefault();

        }

        interceptedTypes.Add(targetType);

    }

    private IComponentFactory GetComponentFactory(Type type) {
        if (type.GetCustomAttribute<ComponentAttribute>() is ComponentAttribute componentAttribute) {
            // Return proxy or smth (Check if component should be instantiated on every call to get component)
            return singletonFactory;
        }
        return singletonFactory;
    }

}
