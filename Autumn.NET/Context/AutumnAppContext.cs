using System.Reflection;
using System.Text;

using Autumn.Annotations;
using Autumn.Annotations.Internal;
using Autumn.Context.Configuration;
using Autumn.Context.Factory;
using Autumn.Reflection;
using Autumn.Types;

namespace Autumn.Context;

/// <summary>
/// Represents the application context for the Autumn framework.
/// It provides dependency injection and configuration management functionality, 
/// serving as the central part for managing components and their lifecycles.
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
        services = [];
        configurationInstances = [];
        configSources = [];
        components = [];
        interceptedTypes = [];
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
        if (services.TryGetValue(qualifier, out HashSet<TypeCreator>? value)) {
            value.Add(creator);
            return;
        }
        services.Add(qualifier, [creator]);
    }

    /// <summary>
    /// Registers a component type to be resolved by the application context.
    /// It associates the component with a factory responsible for creating its instances.
    /// </summary>
    /// <typeparam name="T">The type of the component to register</typeparam>
    public void RegisterComponent<T>() => RegisterComponent(typeof(T));

    /// <summary>
    /// Registers a component type to be resolved by the application context.
    /// It associates the component with a factory responsible for creating its instances.
    /// </summary>
    /// <param name="component">The type of the component to register.</param>
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
        if (creator.Type.IsAbstract || creator.Type.IsInterface) {
            components.Add(identifier.ComponentQualifier, new()); // Could go very bad if we tried to make an instance of an abstract/interface class
        } else {
            components.Add(identifier.ComponentQualifier, new() { creator });
        }
    }

    /// <summary>
    /// Register a specific component instance in the <see cref="AutumnAppContext"/>.
    /// </summary>
    /// <typeparam name="T">The type of component to register</typeparam>
    /// <param name="component">The instance to register</param>
    public void RegisterComponent<T>(T component) where T : notnull {
        Type componentType = component.GetType();
        RegisterComponent(componentType);
        var identifier = ComponentIdentifier.DefaultIdentifier(componentType);
        singletonFactory.RegisterSingleton(identifier, component);
    }

    /// <summary>
    /// Register a specific component instance in the <see cref="AutumnAppContext"/> with a specific qualifier.
    /// </summary>
    /// <typeparam name="T">The type of component to register</typeparam>
    /// <param name="component">The instance to register</param>
    /// <param name="componentQualifier">The qualifier for the component</param>
    public void RegisterComponent<T>(T component, string componentQualifier) where T : notnull {
        RegisterComponent(component);
        var identifier = new ComponentIdentifier(componentQualifier, component.GetType());
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
    public object? GetInstanceOf(Type type) => GetInstanceOfInternal(type, Array.Empty<object>());

    /// <summary>
    /// Retrieves an instance of the specified type from the application context.
    /// </summary>
    /// <param name="type">The type of the instance to retrieve.</param>
    /// <param name="args">The arguments to use when constructing the instance</param>
    /// <returns>The instance of the specified type, if found; otherwise, null.</returns>
    public object? GetInstanceOf(Type type, params object[] args) => GetInstanceOfInternal(type, args);

    /// <summary>
    /// Retrieves an instance of the specified type from the application context.
    /// </summary>
    /// <typeparam name="T">The type of the instance to retrieve</typeparam>
    /// <returns>The instance of the specified type, if found; otherwise, null.</returns>
    public T GetInstanceOf<T>() => GetInstanceOfInternal(typeof(T), Array.Empty<object>()) is T t ? t : throw new ComponentNotFoundException(typeof(T));

    /// <summary>
    /// Retrieves an instance of the specified type from the application context.
    /// </summary>
    /// <typeparam name="T">The type of the instance to retrieve</typeparam>
    /// <param name="constructorArguments">The arguments to pass on to the constructor.</param>
    /// <returns>The instance of the specified type, if found; otherwise, null.</returns>
    public T GetInstanceOf<T>(params object[] constructorArguments) => GetInstanceOfInternal(typeof(T), constructorArguments) is T t ? t : throw new ComponentNotFoundException(typeof(T));

    private static bool IsSoughtAfterComponent(Type currentType, Type targetType) {
        if (currentType == targetType)
            return true;
        if (targetType.IsInterface && currentType.IsAssignableTo(targetType))
            return true;
        return false;
    }

    private object? GetInstanceOfInternal(Type type, object[] constructorArguments) {

        var key = ComponentIdentifier.DefaultIdentifier(type);
        if (components.TryGetValue(key.ComponentQualifier, out var instance) && instance.FirstOrDefault(x => IsSoughtAfterComponent(x.Type,type)) is TypeCreator creator) {
            return creator.Factory.GetComponent(key, constructorArguments, null);
        }

        if (services.TryGetValue(type.FullName!, out var instances) && instances.Any(x => x.Type == type)) {
            //return CreateService(type);
        }

        return null;

    }

    internal delegate (bool, object?) InjectParameterHandler(IInjectAnnotation annotation, ParameterInfo parameterInfo);

    internal object? CreateContextObject(Type componentType, object[] args)
        => CreateContextObjectInternal(componentType, null, null, args, null);

    internal object? CreateContextObject(Type componentType, Action<object>? constructed, object[] args) 
        => CreateContextObjectInternal(componentType, null, constructed, args, null);

    internal object? CreateContextObject(Type componentType, Action<object>? constructed, object[] args, IScopeContext? scopeContext)
        => CreateContextObjectInternal(componentType, null, constructed, args, scopeContext);

    internal object? CreateContextObject(Type componentType, InjectParameterHandler? injectionHandler, Action<object>? constructed, object[] args)
        => CreateContextObjectInternal(componentType, injectionHandler, constructed, args, null);

    private object? CreateContextObjectInternal(Type componentType, InjectParameterHandler? injectionHandler, Action<object>? constructed, object[] args, IScopeContext? scopeContext) {

        // Get best constructor
        var (ctor, callargs) = GetConstructor(componentType, injectionHandler, args, scopeContext);
        if (ctor is null) {
            return null;
        }

        // Create
        var component = ctor.Instantiate(callargs) ?? throw new Exception();
        constructed?.Invoke(component);

        // Initialise
        InitialiseContextObject(component, componentType, scopeContext);

        // Return the component
        return component;

    }

    internal void InitialiseContextObject(object component, Type componentType, IScopeContext? scopeContext) {

        // Setup values
        ApplyValueAttribute(componentType, component, configSources);

        // Inject dependencies
        InjectDependencies(component, componentType, scopeContext);

        // Trigger post init
        RunPostInit(component, componentType);

    }

    private (ObjectInstantiator?, object?[] callargs) GetConstructor(Type klass, InjectParameterHandler? injectHandler, object[] args, IScopeContext? scopeContext) {

        // Get factory constructor
        if (GetFactoryClassConstructor(klass, injectHandler, args, scopeContext) is (FactoryInstantiator fi, object?[] factoryArgs)) {
            return (fi, factoryArgs);
        }

        // Get constructors
        var ctors = klass.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        List<(ConstructorInfo?, object?[], int)> candidates = new();
        foreach (var ctor in ctors) {
            var ctorArgs = ctor.GetParameters();
            if (ctor.GetCustomAttribute<InjectAttribute>() is not null) {
                throw new NotImplementedException();
            } else {
                var attributedArgs = ctorArgs.Select(x => (x, GetInjectAttribute(x))).ToArray();
                var injectArgs = attributedArgs.Where(x => x.Item2 is not null).ToArray();
                var passArgs = attributedArgs.Where(x => x.Item2 is null).ToArray();
                if (passArgs.Length == args.Length && ctorArgs.Length == args.Length && passArgs.Zip(args).All(x => x.Second.GetType().IsAssignableTo(x.First.x.ParameterType))) {
                    candidates.Add((ctor, args, 0));
                    continue;
                }
                var callArgs = new object?[ctorArgs.Length];
                int j = 0;
                for (int i = 0; i < callArgs.Length; i++) {
                    if (attributedArgs[i].Item2 is InjectAttribute inject) {
                        callArgs[i] = SolveInjectDependencyInternal(attributedArgs[i].x.ParameterType, attributedArgs[i].x.Name!, inject, scopeContext);
                    } else if (injectHandler is not null && attributedArgs[i].Item2 is IInjectAnnotation iij) {
                        var (found, value) = injectHandler(iij, attributedArgs[i].x);
                        callArgs[i] = found ? value : passArgs[j++];
                    } else if (j < args.Length && args[j].GetType().IsAssignableTo(ctorArgs[i].ParameterType)) {
                        callArgs[i] = args[j++];
                    }
                }
                candidates.Add((ctor, callArgs, 1));
            }
        }
        if (candidates.Count is 1) {
            var ctorInfo = candidates[0].Item1;
            return ctorInfo is not null ? (new ConstructorInstantiator(ctorInfo), candidates[0].Item2) : (null, []);
        } else if (candidates.Count > 1) {
            var best = candidates[0];
            for (int i = 1; i < candidates.Count; i++) {
                if (candidates[i].Item3 < best.Item3)
                    best = candidates[i];
            }
            var ctorInfo = best.Item1;
            return ctorInfo is not null ? (new ConstructorInstantiator(ctorInfo), best.Item2) : (null, []);
        }
        return (null, []);

    }

    private static (ObjectInstantiator?, object?[]) GetFactoryClassConstructor(Type klass, InjectParameterHandler? injectHandler, object[] args, IScopeContext? scopeContext) {

        var factories = klass.GetCustomAttributes<FactoryAttribute>().ToArray();
        if (factories.Length > 0) {
            throw new NotSupportedException("Factory classes not yet implemented");
        }

        return (null, []);

    }

    private static Attribute? GetInjectAttribute(ParameterInfo p) {
        Attribute? att = null;
        foreach (var a in p.GetCustomAttributes()) {
            if (a is IInjectAnnotation) {
                if (att is not null)
                    throw new Exception(); // Illegal annotation combination
                att = a;
            }
        }
        return att;
    }

    private object? GetComponent((string,Type) componentIdentifier, IScopeContext? scopeContext) {
        var identifier = new ComponentIdentifier(componentIdentifier.Item1, componentIdentifier.Item2);
        return GetComponentInternal(identifier, scopeContext);
    }

    private object? GetComponentInternal(ComponentIdentifier identifier, IScopeContext? scopeContext) {
        if (singletonFactory.HasSingleton(identifier)) {
            return singletonFactory.GetComponent(identifier, [], scopeContext);
        }
        if (components.TryGetValue(identifier.ComponentQualifier, out var typesByQualifier)) {
            throw new NotImplementedException();
        }
        if (components.TryGetValue(identifier.ComponentInstanceType.FullName!, out var typesByType)) {
            var qualifiedByName = typesByType.Where(x => x.Type.Name == identifier.ComponentQualifier).ToList();
            if (qualifiedByName.Count == 1) {
                return qualifiedByName[0].Factory.GetComponent(ComponentIdentifier.DefaultIdentifier(qualifiedByName[0].Type), [], scopeContext);
            }
            if (typesByType.Count == 1) {
                return typesByType.First().Factory.GetComponent(ComponentIdentifier.DefaultIdentifier(typesByType.First().Type), [], scopeContext);
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
            InjectionHelper.Inject(klass, propertyInfo, instance, value);
        }

    }

    private bool LookupConfigValue(Type requestedType, ICollection<IConfigSource> sources, string key, string? defaultValue, out object? value) {

        value = null;
        foreach (var source in sources) {
            if (source.HasValue(key)) {
                value = source.GetValue(key);
                if (value is null) {
                    return false;
                }
                if (value.GetType() != requestedType) {
                    value = TryChangeType(value, requestedType);
                }
                return true;
            }
        }

        if (!string.IsNullOrEmpty(defaultValue) && TryChangeType(defaultValue, requestedType) is object someValue) {
            value = someValue;
            return true;
        }

        return false;

    }

    private static object? TryChangeType(object value, Type targetType) => TypeConverter.Convert(value, value.GetType(), targetType);

    /// <summary>
    /// Injects dependencies into the target object based on the <see cref="InjectAttribute"/> of its properties.
    /// </summary>
    /// <param name="target">The object into which dependencies are to be injected.</param>
    /// <param name="targetType">The type of the target object.</param>
    /// <param name="scopeContext">The scope context, if any, associated with the target object.</param>
    private void InjectDependencies(object target, Type targetType, IScopeContext? scopeContext) {

        // Get properties
        var injectProperties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(x => (x, x.GetCustomAttribute<InjectAttribute>())).Where(x => x.Item2 is not null);

        // Loop over all properties
        foreach (var property in injectProperties) {

            // Get inject value
            object? injectValue = SolveInjectDependencyInternal(property.x.PropertyType, property.x.Name, property.Item2!, scopeContext);

            // Set it
            InjectionHelper.Inject(targetType, property.x, target, injectValue);

        }

    }

    /// <summary>
    /// Resolves and returns a dependency for the given type and qualifier.
    /// </summary>
    /// <param name="expectedType">The type of the dependency to resolve.</param>
    /// <param name="implicitQualifier">The implicit qualifier used for resolving the dependency.</param>
    /// <param name="inject">The InjectAttribute that specifies additional details for dependency resolution.</param>
    /// <returns>The resolved dependency object if found; otherwise, null.</returns>
    internal object? SolveInjectDependency(Type expectedType, string implicitQualifier, InjectAttribute inject) 
        => SolveInjectDependencyInternal(expectedType, implicitQualifier, inject, null);

    /// <summary>
    /// Resolves and returns a dependency for the given type and qualifier.
    /// </summary>
    /// <param name="expectedType">The type of the dependency to resolve.</param>
    /// <param name="implicitQualifier">The implicit qualifier used for resolving the dependency.</param>
    /// <param name="inject">The InjectAttribute that specifies additional details for dependency resolution.</param>
    /// <param name="scope">The scope context to consider while resolving the dependency.</param>
    /// <returns>The resolved dependency object if found; otherwise, null.</returns>
    internal object? SolveInjectDependency(Type expectedType, string implicitQualifier, InjectAttribute inject, IScopeContext? scope) 
        => SolveInjectDependencyInternal(expectedType, implicitQualifier, inject, scope);

    /// <summary>
    /// Resolves and returns a dependency for the given type and qualifier considering the scope context.
    /// </summary>
    /// <param name="expectedType">The type of the dependency to resolve.</param>
    /// <param name="implicitQualifier">The implicit qualifier used for resolving the dependency.</param>
    /// <param name="inject">The InjectAttribute that specifies additional details for dependency resolution.</param>
    /// <param name="scope">The scope context to consider while resolving the dependency.</param>
    /// <returns>The resolved dependency object if found; otherwise, null.</returns>
    private object? SolveInjectDependencyInternal(Type expectedType, string implicitQualifier, InjectAttribute inject, IScopeContext? scope) {
        if (!string.IsNullOrEmpty(inject.Qualifier)) {
            throw new NotImplementedException();
        }
        if (inject.FactoryArguments is not null) {
            ComponentIdentifier ci = new ComponentIdentifier(implicitQualifier + ArgsAsString(inject.FactoryArguments), expectedType);
            if (singletonFactory.HasSingleton(ci)) { // TODO: Handle scopes at some point, right now it relies on it always being a singleton
                return GetComponent((ci.ComponentQualifier, ci.ComponentInstanceType), scope);
            }
            var factoryMethod = GetFactoryMethod(expectedType, inject, scope);
            if (factoryMethod is not null) {
                var instance = factoryMethod.Invoke(null, inject.FactoryArguments) ?? throw new Exception("Fatal error while trying to invoke factory method");
                singletonFactory.RegisterSingleton(ci, instance);
                AssociateComponent(ci, new TypeCreator(expectedType, singletonFactory));
                return instance;
            }
        }
        var byImplicitQualifier = (implicitQualifier, expectedType);
        if (!string.IsNullOrEmpty(implicitQualifier) && GetComponent(byImplicitQualifier, scope) is object component) {
            return component;
        }
        if (GetComponent((expectedType.FullName!, expectedType), scope) is object fullyQualifiedComponent) {
            return fullyQualifiedComponent;
        }
        return null; // TODO: Check if null
    }

    private MethodInfo? GetFactoryMethod(Type expectedType, InjectAttribute inject, IScopeContext? scope) {
        var factoryArgs = inject.FactoryArguments!;
        var factoryMethods = expectedType.GetMethods(BindingFlags.Static | BindingFlags.Public)
            .Select(x => (x, x.GetCustomAttribute<FactoryMethodAttribute>()))
            .Where(x => x.Item2 is not null)
            .ToArray();
        if (factoryMethods.Length == 0)
            return null;

        MethodInfo? bestFactoryMethod = null;
        for (int i = 0; i < factoryMethods.Length; i++) {
            var parameters = factoryMethods[i].x.GetParameters();
            if (parameters.Length == factoryArgs.Length) {
                bool isCandidate = true;
                for (int j = 0; j < parameters.Length && isCandidate; j++) {
                    var arg = factoryArgs[j];
                    isCandidate &= (arg is null && !parameters[j].ParameterType.IsPrimitive) || (arg is not null && arg.GetType().IsAssignableTo(parameters[j].ParameterType));
                }
                if (isCandidate)
                    bestFactoryMethod = factoryMethods[i].x;
            }
        }
        return bestFactoryMethod;
    }

    /// <summary>
    /// Executes post-initialization methods annotated with <see cref="PostInitAttribute"/> on the provided target object.
    /// </summary>
    /// <param name="target">The object on which post-initialization methods will be executed.</param>
    /// <param name="targetType">The type of the target object.</param>
    private void RunPostInit(object target, Type targetType) {
        var postInits = targetType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(x => (x, x.GetCustomAttribute<PostInitAttribute>()))
                .Where(x => x.Item2 is not null);
        foreach (var (post, p) in postInits) {
            if (p!.AsyncTask) {
                Task.Run(() => post.Invoke(target, []));
            } else {
                post.Invoke(target, []);
            }
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

            var targetPostProcessor = postprocs.Where(x => x.x == targetMethods[i]).Select(x => x.Item2).FirstOrDefault() ?? [];
            var targetException = exceptionHandlers.Where(x => x.x == targetMethods[i]).Select(x => x.Item2).FirstOrDefault();

        }

        interceptedTypes.Add(targetType);

    }

    /// <summary>
    /// Retrieves the appropriate factory for creating instances of the specified type based on its ComponentAttribute.
    /// Defaults to the singleton factory if no specific attribute is found.
    /// </summary>
    /// <param name="type">The type for which to retrieve the component factory.</param>
    /// <returns>An instance of IComponentFactory responsible for creating instances of the specified type.</returns>
    /// <exception cref="NotImplementedException">Thrown if the specified type's scope is not handled or if proxy creation is required but not implemented.</exception>
    private IComponentFactory GetComponentFactory(Type type) {
        if (type.GetCustomAttribute<ComponentAttribute>() is ComponentAttribute componentAttribute) {
            var makeProxy = false; // TODO: Get better flag
            if (makeProxy) {
                throw new NotImplementedException();
            }
            return componentAttribute.Scope switch {
                ComponentScope.Singleton => singletonFactory,
                ComponentScope.Multiton => new InstanceFactory(this, type),
                ComponentScope.Scoped => new ScopeFactory(this, type),
                _ => throw new NotImplementedException()
            };
        }
        return singletonFactory;
    }

    /// <summary>
    /// Retrieves all component instances of the specified type from the application context.
    /// </summary>
    /// <param name="type">The type of the components to retrieve.</param>
    /// <returns>An array of component instances of the specified type. Returns an empty array if no components are found.</returns>
    public object[] GetComponents(Type type) {
        if (components.TryGetValue(type.FullName!, out var creators)) {
            var instances = creators.Select(x => x.Factory.GetComponent(ComponentIdentifier.DefaultIdentifier(x.Type), [], null)).ToArray();
            return instances;
        }
        return [];
    }

    /// <summary>
    /// Retrieves all component instances of the specified generic type from the application context.
    /// </summary>
    /// <typeparam name="T">The generic type of the components to retrieve.</typeparam>
    /// <returns>An array of component instances of the specified generic type. Returns an empty array if no components are found.</returns>
    public T[] GetComponents<T>() {
        object[] components = GetComponents(typeof(T));
        T[] result = new T[components.Length];
        for (int i = 0; i < components.Length;i++) {
            result[i] = (T)components[i];
        }
        return result;
    }

    /// <summary>
    /// Retrieves the Type object associated with the specified class name, performing registration if necessary.
    /// </summary>
    /// <param name="fullname">The full name of the type to retrieve.</param>
    /// <returns>The Type object for the specified class name if found; otherwise, null.</returns>
    /// <exception cref="NotImplementedException">Thrown when unable to register the type from the given typename.</exception>
    public Type? GetComponentType(string fullname) {
        if (components.TryGetValue(fullname, out var creators)) {
            return creators.FirstOrDefault().Type;
        }
        Type? type = Type.GetType(fullname);
        if (type is not null) {
            RegisterComponent(type);
            return GetComponentType(fullname); // Call ourselves again, there should now be an associated component!
        }
        throw new NotImplementedException("Unable to register type from typename - not implemented");
    }

    /// <summary>
    /// Invokes the given method while injecting any required arguments
    /// </summary>
    /// <param name="instance">The instance to invoke the method on</param>
    /// <param name="method">The method to invoke</param>
    /// <returns>The value returned by the method</returns>
    public object? Invoke(object? instance, MethodInfo method) => Invoke(instance, method, null);

    /// <summary>
    /// Invokes the given method while injecting any required arguments
    /// </summary>
    /// <param name="instance">The instance to invoke the method on</param>
    /// <param name="method">The method to invoke</param>
    /// <param name="scopeContext">The context to use when injecting parameters</param>
    /// <returns>The value returned by the method</returns>
    public object? Invoke(object? instance, MethodInfo method, IScopeContext? scopeContext) {
        ParameterInfo[] parameters = method.GetParameters();
        object?[] args = new object?[parameters.Length];
        for (int i = 0; i < args.Length; i++) {
            args[i] = SolveInjectDependencyInternal(parameters[i].ParameterType, parameters[i].Name!, parameters[i].GetCustomAttribute<InjectAttribute>() ?? new InjectAttribute(), scopeContext);
        }
        return method.Invoke(instance, args);
    }

    private static string ArgsAsString(object?[] args) { 
        StringBuilder sb = new StringBuilder('[');
        for (int i = 0; i < args.Length; i++) {
            sb.Append(args[i]);
            if (i + 1 < args.Length) {
                sb.Append(',');
            }
        }
        sb.Append(']');
        return sb.ToString();
    }

}
