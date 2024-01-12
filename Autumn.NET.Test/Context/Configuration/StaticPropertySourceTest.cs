using System.Text;

using Autumn.Annotations;
using Autumn.Context.Configuration;

namespace Autumn.Test.Context.Configuration;

public class StaticPropertySourceTest {

    private record Person([ConfigName("name")] string Name, int Age);

    [Fact]
    public void CanParseToComplexType() {

        string StringArrayYaml = """
        person:
            name: "John Doe"
            age: 52
        """;

        ConfigFactory factory = new ConfigFactory();
        var src = factory.LoadConfig("cfgtest.yml", new MemoryStream(Encoding.UTF8.GetBytes(StringArrayYaml)));

        Assert.NotNull(src);
        Assert.IsType<StaticPropertySource>(src);

        StaticPropertySource staticPropertySource = (StaticPropertySource)src;

        Person? person = staticPropertySource.GetValue<Person>("person");
        Assert.NotNull(person);
        Assert.Equal("John Doe", person.Name);
        Assert.Equal(52, person.Age);

    }

    [Fact]
    public void CanParseToComplexArrayType() {

        string StringArrayYaml = """
        people:
            - name: "John Doe"
              age: 52
            - name: "Jane Doe"
              age: 50
        """;

        ConfigFactory factory = new ConfigFactory();
        var src = factory.LoadConfig("cfgtest.yml", new MemoryStream(Encoding.UTF8.GetBytes(StringArrayYaml)));

        Assert.NotNull(src);
        Assert.IsType<StaticPropertySource>(src);

        StaticPropertySource staticPropertySource = (StaticPropertySource)src;
        Person[]? person = staticPropertySource.GetValue<Person[]>("people");
        Assert.NotNull(person);
        Assert.Equal(2, person.Length);

        Assert.Equal("John Doe", person[0].Name);
        Assert.Equal(52, person[0].Age);

        Assert.Equal("Jane Doe", person[1].Name);
        Assert.Equal(50, person[1].Age);

    }

}
