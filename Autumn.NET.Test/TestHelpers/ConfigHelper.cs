using System.Text;

using Autumn.Context.Configuration;

namespace Autumn.Test.TestHelpers;

public static class ConfigHelper {
    public static readonly Func<string, StaticPropertySource> ConfigOf = x => (StaticPropertySource)(new ConfigFactory().LoadConfig("application.yaml", new MemoryStream(Encoding.UTF8.GetBytes(x))))!;
}
