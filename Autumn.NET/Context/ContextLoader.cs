using System.Reflection;
using System.Runtime.InteropServices;

using Autumn.Annotations;
using Autumn.Annotations.Library;
using Autumn.Context.Configuration;
using Autumn.Functional;

namespace Autumn.Context;

internal sealed class ContextLoader {

    private record struct AutumnTemplate(Type Target, AutumnTemplateAttribute TemplateAttribute);

    private record struct AutumnLibraryContextLoader(Type ClassTarget, Action<AutumnAppContext, Type[]> MethodTarget, AutumnContextLoaderAttribute ContextLoaderAttribute);

    private readonly string[] propertySources;
    private readonly ConfigFactory configFactory;
    private readonly Dictionary<Type, AutumnTemplate> templates;
    private readonly Dictionary<Type, AutumnTemplate> templateImplementations;
    private readonly List<AutumnLibraryContextLoader> libraryContextLoaders;

    internal ContextLoader(params string[] propertySources) { 
        this.propertySources = propertySources
            .Prepend("application.yml")
            .ToArray();
        this.configFactory = new();
        this.templates = new();
        this.templateImplementations = new();
        this.libraryContextLoaders = new List<AutumnLibraryContextLoader>();
    }

    internal void LoadAssemblyContext() {

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

        var domainTypes = assemblyDictionary.Values.SelectMany(x => x.GetExportedTypes()).ToArray();
        // TODO: Use these types

        // Load Autumn assemblies
        LoadAutumnAssemblies(projectAssemblies);

    }

    private void LoadAutumnAssemblies(HashSet<Assembly> loadedAssemblies) {
        // TODO: Add security checks here to verify these assemblies are loaded correctly
        string[] autumnAssemblyFiles = {
            "Autumn.NET.Database.dll",
            "Autumn.NET.Database.*.dll" // Database specific implementations
        };
        HashSet<string> targetAssemblies = new HashSet<string>();
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
        HashSet<Assembly> autumnAssemblies = new HashSet<Assembly>();
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
                    new Type[] { 
                        typeof(AutumnAppContext), 
                        typeof(Type[]) 
                    });
                if (targetMethod is not null)
                    libraryContextLoaders.Add(new AutumnLibraryContextLoader(type, targetMethod.CreateDelegate<Action<AutumnAppContext, Type[]>>(), autumnContextLoaderAttribute));
            }
        }

    }

    internal void LoadContext(AutumnAppContext context, Type[] types) {

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

        // Load components
        LoadComponents(context, componentTypes);

        // Load services
        LoadServices(context, serviceTypes);

        // Load context of custom autumn loaders
        foreach (var libraryLoaders in libraryContextLoaders) {
            libraryLoaders.MethodTarget.Invoke(context, types);
        }

    }

    private void LoadConfiguration(AutumnAppContext context, IEnumerable<(Type, ConfigurationAttribute?)> configurationTypes) {

        // Load all sources
        List<IConfigSource> sources = new List<IConfigSource>();

        // Load applications
        for (int i = 0; i < propertySources.Length; i++) {
            if (File.Exists(propertySources[i])) {
                using var fs = File.OpenRead(propertySources[i]);
                if (configFactory.LoadConfig(propertySources[i], fs) is IConfigSource source) {
                    sources.Add(source);
                    context.RegisterComponent(source); // Also expose it to possible users
                }
            }
        }

        // Save
        context.RegisterConfigSource(sources.ToArray());

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

}
