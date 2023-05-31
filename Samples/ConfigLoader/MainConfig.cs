using Autumn.Annotations;

namespace ConfigLoader;

[Configuration]
public class MainConfig {

    [Value("myconfig.name")]
    public string Title { get; }

    [Value("myconfig.desc")]
    public string Description { get; }

    [Value("myconfig.version")]
    public string Version { get; }

    [Value("myconfig.project", Default = "Autumn")]
    public string Project { get; }

}
