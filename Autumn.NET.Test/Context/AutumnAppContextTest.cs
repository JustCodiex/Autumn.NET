using Autumn.Context.Configuration;
using Autumn.Context;
using Autumn.Annotations;

namespace Autumn.Test.Context;

public class AutumnAppContextTest {

    [Fact]
    public void WillGetDefaultComponentFromInterface() {

        AutumnAppContext appContext = new AutumnAppContext();
        appContext.RegisterComponent(new StaticPropertySource(new Dictionary<string, object?>()));

        IConfigSource obj = appContext.GetInstanceOf<IConfigSource>();
        Assert.NotNull(obj);
        Assert.IsType<StaticPropertySource>(obj);

    }

    public class SampleService {
        public string Name { get; set; } = "SampleService";
    }

    public class SampleComponent {
        public string ComponentName { get; set; } = "SampleComponent";
    }

    public class SampleConfig {
        public string ConfigKey { get; set; } = "ConfigValue";
    }

    public class ComponentWithDependency {
        [Inject]
        public SampleService Dependency { get; set; } = default!;
    }

    [Fact]
    public void RegisterService_ShouldRegisterServiceType() {
        // Arrange
        var context = new AutumnAppContext();

        // Act
        context.RegisterService<SampleService>();

        // Assert
        var services = context.GetServices();
        Assert.Contains(typeof(SampleService), services);
    }

    [Fact]
    public void GetInstanceOf_ShouldReturnRegisteredService() {
        // Arrange
        var context = new AutumnAppContext();
        context.RegisterService<SampleService>();

        // Act
        var instance = context.GetInstanceOf<SampleService>();

        // Assert
        Assert.NotNull(instance);
        Assert.IsType<SampleService>(instance);
    }

    [Fact]
    public void RegisterComponent_ShouldRegisterAndReturnComponent() {
        // Arrange
        var context = new AutumnAppContext();
        var component = new SampleComponent();

        // Act
        context.RegisterComponent(component);
        var retrievedComponent = context.GetInstanceOf<SampleComponent>();

        // Assert
        Assert.Equal(component, retrievedComponent);
    }

    [Fact]
    public void RegisterConfiguration_ShouldStoreConfigurationInstance() {
        // Arrange
        var context = new AutumnAppContext();
        var config = new SampleConfig();

        // Act
        context.RegisterConfiguration(typeof(SampleConfig), config);

        // Assert
        var result = context.GetInstanceOf<SampleConfig>();
        Assert.NotNull(result);
        Assert.Equal(config, result);
    }

    [Fact]
    public void InjectDependencies_ShouldInjectDependenciesCorrectly() {
        // Arrange
        var context = new AutumnAppContext();
        var component = new ComponentWithDependency();
        context.RegisterService<SampleService>(); // Register the dependency

        // Act
        context.InjectDependencies(component, component.GetType(), null);

        // Assert
        Assert.NotNull(component.Dependency);
        Assert.IsType<SampleService>(component.Dependency);
    }

}
