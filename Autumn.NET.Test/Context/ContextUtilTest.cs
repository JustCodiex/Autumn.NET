using Autumn.Context;
using Autumn.Context.Configuration;

namespace Autumn.Test.Context;

public class ContextUtilTest {

    private interface ITestInterface {}
    private class DefaultComponent : ITestInterface {}
    private class SpecificComponent : ITestInterface {}

    [Fact]
    public void WillGetDefault() {

        AutumnAppContext appContext = new AutumnAppContext();
        appContext.RegisterComponent(new StaticPropertySource(new Dictionary<string, object?>()));
        appContext.RegisterComponent<DefaultComponent>();

        ITestInterface obj = ContextUtil.GetOrCreateComponentFromProperty<ITestInterface, DefaultComponent>(appContext, "this.does.not.exist");
        Assert.NotNull(obj);
        Assert.IsType<DefaultComponent>(obj);

    }

    [Fact]
    public void WillGetSpecific() {

        AutumnAppContext appContext = new AutumnAppContext();
        appContext.RegisterComponent(new StaticPropertySource(new Dictionary<string, object?>() { { "my.type", typeof(SpecificComponent).FullName } }));
        appContext.RegisterComponent<DefaultComponent>();
        appContext.RegisterComponent<SpecificComponent>();

        ITestInterface obj = ContextUtil.GetOrCreateComponentFromProperty<ITestInterface, DefaultComponent>(appContext, "my.type");
        Assert.NotNull(obj);
        Assert.IsType<SpecificComponent>(obj);

    }

}
