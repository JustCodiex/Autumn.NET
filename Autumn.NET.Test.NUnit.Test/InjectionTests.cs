using Autumn.Annotations;

namespace Autumn.NET.Test.NUnit.Test;

[AutumnTest]
public class InjectionTests {

    [Inject]
    public TestComponent? Component { get; set; }

    [SetUp]
    public void Setup() {
        Assert.That(Component, Is.Not.Null);
    }

    [Test]
    public void TestCanGetInjectedComponentValue() {
        Assert.That(Component!.GetInt(), Is.EqualTo(5));
    }

}
