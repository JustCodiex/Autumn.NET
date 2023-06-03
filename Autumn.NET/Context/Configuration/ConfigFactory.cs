using System.Globalization;

using YamlDotNet.RepresentationModel;

namespace Autumn.Context.Configuration;

/// <summary>
/// Factory class for loading configuration from different sources.
/// </summary>
public sealed class ConfigFactory {

    /// <summary>
    /// Loads configuration from a stream based on the source identifier.
    /// </summary>
    /// <param name="sourceIdentifier">The identifier of the configuration source.</param>
    /// <param name="source">The stream representing the configuration source.</param>
    /// <returns>The loaded configuration as an <see cref="IConfigSource"/> instance, or null if the source is not supported.</returns>
    public IConfigSource? LoadConfig(string sourceIdentifier, Stream source) {
        switch (Path.GetExtension(sourceIdentifier)) {
            case ".yml":
            case ".yaml": {
                using TextReader reader = new StreamReader(source);
                YamlStream ymlStream = new YamlStream();
                ymlStream.Load(reader);
                var cfg = new Dictionary<string, object?>();
                foreach (var doc in ymlStream.Documents) {
                    HandleYamlNode(string.Empty, doc.RootNode, cfg);
                }
                return new StaticPropertySource(cfg);
            }
            default:
                return null;
        }
    }

    private void HandleYamlNode(string path, YamlNode node, Dictionary<string, object?> cfg) {
        switch (node) {
            case YamlMappingNode mappingNode:
                foreach (var (key, value) in mappingNode) {
                    if (key is YamlScalarNode keyScalar) {
                        HandleYamlNode(path == string.Empty ? keyScalar.Value! : path + "." + keyScalar.Value!, value, cfg);
                    } else {
                        throw new NotImplementedException();
                    }
                }
                break;
            case YamlScalarNode scalarNode:
                if (int.TryParse(scalarNode.Value!, out int i)) {
                    cfg[path] = i;
                } else if (double.TryParse(scalarNode.Value!, CultureInfo.InvariantCulture, out double d)) {
                    cfg[path] = d;
                } else if (scalarNode.Value?.ToLower() is "true") {
                    cfg[path] = true;
                } else if (scalarNode.Value?.ToLower() is "false") {
                    cfg[path] = false;
                } else {
                    cfg[path] = scalarNode.Value;
                }
                break;
            default:
                throw new NotImplementedException();
        }
    }

}
