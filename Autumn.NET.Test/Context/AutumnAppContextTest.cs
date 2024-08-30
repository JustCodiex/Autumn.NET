using Autumn.Context.Configuration;
using Autumn.Context;

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

}
