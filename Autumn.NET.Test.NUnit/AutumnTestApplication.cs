using Autumn.Context;

namespace Autumn.NET.Test.NUnit;

internal class AutumnTestApplication {

    private static AutumnTestApplication? instance;

    private AutumnAppContext appContext;

    public AutumnTestApplication() { 
        this.appContext = new AutumnAppContext();
    }

    internal void LoadContext(object initialFixture) {

        // Create context loader
        ContextLoader loader = new ContextLoader();
        var subTypes = loader.GetTypes(initialFixture.GetType());

        // Load it up
        loader.LoadAssemblyContext(appContext);
        loader.LoadContext(appContext, subTypes);

    }

    internal void InitFixture(object fixture) {
        appContext.InitialiseContextObject(fixture, fixture.GetType(), null); // TODO: Replace null with a test context object
    }

    public static AutumnTestApplication GetTestApplication(object initialFixture) {
        if (instance is null) {
            instance = new AutumnTestApplication();
            instance.LoadContext(initialFixture);
        }
        return instance;
    }

}
