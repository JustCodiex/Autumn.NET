using System.Reflection;

using Autumn.Annotations;
using Autumn.Context.Configuration;

namespace Autumn.Context;

internal sealed class ContextLoader {

    private readonly string[] propertySources;
    private readonly ConfigFactory configFactory;

    internal ContextLoader(params string[] propertySources) { 
        this.propertySources = propertySources
            .Prepend("application.yml")
            .ToArray();
        this.configFactory = new();
    }

    internal void LoadContext(AutumnAppContext context, Type[] types) {

        // Select all service types
        var serviceTypes = types.Select(x => (x, x.GetCustomAttribute<ServiceAttribute>())).Where(x => x.Item2 is not null);

        // Select all configuration types
        var configTypes = types.Select(x => (x, x.GetCustomAttribute<ConfigurationAttribute>())).Where(x => x.Item2 is not null);

        // Select all component types
        var componentTypes = types.Select(x => (x, x.GetCustomAttribute<ComponentAttribute>())).Where(x => x.Item2 is not null);

        // Load configs
        LoadConfiguration(context, configTypes);

        // Load components
        LoadComponents(context, componentTypes);

        // Load services
        LoadServices(context, serviceTypes);

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
