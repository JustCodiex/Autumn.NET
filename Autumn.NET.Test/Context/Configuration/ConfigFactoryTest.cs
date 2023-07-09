using System.Text;

using Autumn.Context.Configuration;

namespace Autumn.Test.Context.Configuration;

public class ConfigFactoryTest {

    public static readonly string StringArrayYaml = """
        test:
            - a
            - b
            - c
        """;

    [Fact]
    public void CanDeserialiseStringArray() {

        ConfigFactory factory = new ConfigFactory();
        var src = factory.LoadConfig("cfgtest.yml", new MemoryStream(Encoding.UTF8.GetBytes(StringArrayYaml)));

        Assert.NotNull(src);
        Assert.Equal(3, src.GetValue("test.#"));

    }

}
