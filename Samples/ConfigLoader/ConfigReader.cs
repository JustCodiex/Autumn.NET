using Autumn.Annotations;

namespace ConfigLoader;

[Service]
public class ConfigReader {

    public MainConfig MainConfig { get; set; }

    public ConfigReader([Inject] MainConfig mainConfig) {
        this.MainConfig = mainConfig;
    }

    [Start]
    public void PrintConfig() {
        Console.WriteLine("Title: {0}", MainConfig.Title);
        Console.WriteLine("Description: {0}", MainConfig.Description);
        Console.WriteLine("Project: {0}", MainConfig.Project);
        Console.WriteLine("Version: {0}", MainConfig.Version);
    }

}
