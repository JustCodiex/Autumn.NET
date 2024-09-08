using System.Reflection;
using System.Runtime.InteropServices;

using Autumn.Annotations;
using Autumn.Annotations.Library;
using Autumn.Context.Configuration;
using Autumn.Functional;

namespace Autumn.Context;

internal sealed class ContextLoader {

    private record struct AutumnTemplate(Type Target, AutumnTemplateAttribute TemplateAttribute);

    private record struct AutumnLibraryContextLoader(Type ClassTarget, Action<AutumnAppContext, IList<Type>> MethodTarget, AutumnContextLoaderAttribute ContextLoaderAttribute);

    internal record struct AutumnLibraryAppLoader(Type ClassTarget, AutumnApplicationLoaderAttribute LoaderAttribute);

    private readonly string[] propertySources;
    private readonly ConfigFactory configFactory;
    private readonly Dictionary<Type, AutumnTemplate> templates;
    private readonly Dictionary<Type, AutumnTemplate> templateImplementations;
    private readonly List<AutumnLibraryContextLoader> libraryContextLoaders;
    private readonly List<AutumnLibraryAppLoader> libraryAppLoaders;
    private readonly List<string> scanNamespaces;

    internal List<AutumnLibraryAppLoader> ApplicationLoaders => libraryAppLoaders;

    internal ContextLoader(params string[] propertySources) { 
        this.propertySources = propertySources
            .Prepend("application.yml").Prepend("application.yaml")
            .ToArray();
        this.configFactory = new();
        this.templates = [];
        this.templateImplementations = [];
        this.libraryContextLoaders = [];
        this.libraryAppLoaders = [];
        this.scanNamespaces = [];
    }

    internal void LoadAssemblyContext(AutumnAppContext appContext) {

        string runtimeEnvironment = RuntimeEnvironment.GetRuntimeDirectory();

        var assemblyDictionary = AppDomain.CurrentDomain.GetAssemblies().ToStream().Filter(x => !x.Location.StartsWith(runtimeEnvironment)).ToDictionary(x => x.FullName!);
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
            foreach (var refAssembly in assembly.GetReferencedAssemblies()) {
                if (assemblyDictionary.ContainsKey(refAssembly.FullName)) {
                    continue;
                }
                try {
                    var referencedAssembly = Assembly.Load(refAssembly);
                    if (referencedAssembly.Location.StartsWith(runtimeEnvironment)) {
                        continue;
                    }
                    assemblyDictionary.Add(refAssembly.FullName, referencedAssembly);
                } catch {
                    throw;
                }
            }
        }

        // Get relevant assemblies
        var projectAssemblies = new HashSet<Assembly>(assemblyDictionary.Values);

        // Load Autumn assemblies
        LoadAutumnAssemblies(projectAssemblies);

        // Get domain types and load them as well
        var domainTypes = assemblyDictionary.Values
            .SelectMany(x => x.GetExportedTypes())
            .Where(x => !string.IsNullOrEmpty(x.Namespace) && ((scanNamespaces.FindIndex(x.Namespace.StartsWith) is int i && i != -1) || scanNamespaces.Count == 0));

        // Load components
        var components = domainTypes.Select(x => (x, x.GetCustomAttribute<ComponentAttribute>()))
            .Where(x => x.Item2 is not null);
        LoadComponents(appContext, components);

        // Load services
        var services = domainTypes.Select(x => (x, x.GetCustomAttribute<ServiceAttribute>()))
            .Where(x => x.Item2 is not null);
        LoadServices(appContext, services);

    }

    private void LoadAutumnAssemblies(HashSet<Assembly> loadedAssemblies) {
        // TODO: Add security checks here to verify these assemblies are loaded correctly and from a trusted source
        string[] autumnAssemblyFiles = [
            "Autumn.NET.Database.dll",
            "Autumn.NET.Database.*.dll", // Database specific implementations
            "Autumn.NET.WPF.dll"
        ];
        HashSet<string> targetAssemblies = [];
        for (int i = 0; i < autumnAssemblyFiles.Length; i++) {
            if (autumnAssemblyFiles[i].Contains('*')) {
                string[] candidates = Directory.GetFiles(Environment.CurrentDirectory, autumnAssemblyFiles[i]);
                foreach (var candidate in candidates) {
                    targetAssemblies.Add(candidate);
                }
            } else {
                string absolutePath = Path.GetFullPath(autumnAssemblyFiles[i]);
                if (File.Exists(absolutePath)) {
                    targetAssemblies.Add(absolutePath);
                }
            }
        }

        // Load autumn specific assemblies
        HashSet<Assembly> autumnAssemblies = [];
        foreach (var targetAssembly in targetAssemblies) {
            if (loadedAssemblies.FirstOrDefault(x => x.Location == targetAssembly) is Assembly preloadedAssembly) { 
                autumnAssemblies.Add(preloadedAssembly);
            } else {
                try {
                    Assembly assembly = Assembly.LoadFrom(targetAssembly);
                    autumnAssemblies.Add(assembly);
                } catch { }
            }
        }

        // Load them
        foreach (var assembly in autumnAssemblies) {
            LoadAutumnAssembly(assembly);
        }

    }

    private void LoadAutumnAssembly(Assembly assembly) {

        Type[] assemblyTypes = assembly.GetTypes();
        foreach (Type type in assemblyTypes) {
            if (type.GetCustomAttribute<AutumnTemplateAttribute>() is AutumnTemplateAttribute templateAttribute) {
                templates.Add(type, new AutumnTemplate(type, templateAttribute));
            }
            if (type.GetCustomAttribute<AutumnTemplateImplementationAttribute>() is AutumnTemplateImplementationAttribute templateImplementationAttribute) {
                var template = new AutumnTemplate(templateImplementationAttribute.TemplateType, 
                    templateImplementationAttribute.TemplateType.GetCustomAttribute<AutumnTemplateAttribute>() ?? throw new Exception());
                templateImplementations.Add(type, template);
            }
            if (type.GetCustomAttribute<AutumnContextLoaderAttribute>() is AutumnContextLoaderAttribute autumnContextLoaderAttribute) {
                MethodInfo? targetMethod = type.GetMethod("LoadContext", 
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    [
                        typeof(AutumnAppContext), 
                        typeof(IList<Type>) 
                    ]);
                if (targetMethod is not null)
                    libraryContextLoaders.Add(new AutumnLibraryContextLoader(type, targetMethod.CreateDelegate<Action<AutumnAppContext, IList<Type>>>(), autumnContextLoaderAttribute));
            }
            if (type.GetCustomAttribute<AutumnApplicationLoaderAttribute>() is AutumnApplicationLoaderAttribute autumnApplicationLoaderAttribute) {
                libraryAppLoaders.Add(new(type, autumnApplicationLoaderAttribute));
            }
        }

    }

    internal void LoadContext(AutumnAppContext context, IList<Type> types) {

        // Select all service types
        var serviceTypes = types.Select(x => (x, x.GetCustomAttribute<ServiceAttribute>())).Where(x => x.Item2 is not null);

        // Select all configuration types
        var configTypes = types.Select(x => (x, x.GetCustomAttribute<ConfigurationAttribute>())).Where(x => x.Item2 is not null);

        // Select all component types
        var componentTypes = types
            .Union(templateImplementations.Keys)
            .Select(x => (x, x.GetCustomAttribute<ComponentAttribute>())).Where(x => x.Item2 is not null);

        // Load configs
        LoadConfiguration(context, configTypes);

        // Load built-in types
        LoadBuiltInTypes(context);

        // Load components
        LoadComponents(context, componentTypes);

        // Load services
        LoadServices(context, serviceTypes);

        // Load context of custom autumn loaders
        foreach (var libraryLoaders in libraryContextLoaders) {
            libraryLoaders.MethodTarget.Invoke(context, types);
        }

    }

    private void LoadBuiltInTypes(AutumnAppContext context) {
        context.RegisterComponent(typeof(HttpClient));
    }

    private void LoadConfiguration(AutumnAppContext context, IEnumerable<(Type, ConfigurationAttribute?)> configurationTypes) {

        // Load all sources
        List<IConfigSource> sources = [];

        // Load applications
        for (int i = 0; i < propertySources.Length; i++) {
            if (File.Exists(propertySources[i])) {
                using var fs = File.OpenRead(propertySources[i]);
                if (configFactory.LoadConfig(propertySources[i], fs) is IConfigSource source) {
                    sources.Add(source);
                    context.RegisterComponent(source, Path.GetFileNameWithoutExtension(propertySources[i])); // Also expose it to possible users
                }
            }
        }

        // Save
        context.RegisterConfigSource([.. sources]);

        // Instantiate 
        foreach (var (klass, configDesc) in configurationTypes) {
            var instance = Activator.CreateInstance(klass) ?? throw new Exception();
            context.RegisterConfiguration(klass, instance);
        }

    }

    private void LoadServices(AutumnAppContext context, IEnumerable<(Type, ServiceAttribute?)> serviceTypes) {
        foreach (var (klass, serviceDesc) in serviceTypes) {
            string qualifier = string.IsNullOrEmpty(serviceDesc!.Qualifier) ? klass.FullName! : serviceDesc!.Qualifier;
            context.RegisterService(klass, qualifier);
        }
    }

    private void LoadComponents(AutumnAppContext context, IEnumerable<(Type, ComponentAttribute?)> components) {
        foreach (var (klass, componentDesc) in components) {
            context.RegisterComponent(klass);
        }
    }

    public IList<Type> GetTypes(Type source) {

        foreach (var nsSource in source.GetCustomAttributes<ScanNamespaces>()) {
            scanNamespaces.AddRange(nsSource.Namespaces);
        }

        var assemblyTypes = source.Assembly.GetTypes();
        if (!string.IsNullOrEmpty(source.Namespace)) {
            assemblyTypes = assemblyTypes.Where(x => x.Namespace?.StartsWith(source.Namespace) ?? false).ToArray();
        }
        return assemblyTypes;

    }

}
