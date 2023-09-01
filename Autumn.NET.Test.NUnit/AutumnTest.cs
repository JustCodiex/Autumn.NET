using NUnit.Engine.Extensibility;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Autumn.NET.Test.NUnit;

[Extension]
[AttributeUsage(AttributeTargets.Class)]
public class AutumnTest : Attribute, ITestAction {

    public ActionTargets Targets => ActionTargets.Test;

    public void AfterTest(ITest test) {

    }

    public void BeforeTest(ITest test) {
        
        // Grab the target
        var fixture = test.Fixture;
        if (fixture is null) {
            return;
        }

        // Get the application
        var testApp = AutumnTestApplication.GetTestApplication(fixture);

        // Setup the fixture object
        testApp.InitFixture(fixture);

    }

}
