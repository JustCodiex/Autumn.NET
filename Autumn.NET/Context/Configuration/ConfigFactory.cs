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
                cfg[path] = MapScalarToValue(scalarNode);
                break;
            case YamlSequenceNode sequenceNode:
                IList<YamlScalarNode> scalars = new List<YamlScalarNode>();
                for (int n = 0; n < sequenceNode.Children.Count; n++) {
                    HandleYamlNode(path + "." + (n+1), sequenceNode.Children[n], cfg);
                    if (sequenceNode.Children[n] is YamlScalarNode scalar) {
                        scalars.Add(scalar);
                    }
                }
                cfg[path + ".#"] = sequenceNode.Children.Count;
                if (scalars.Count == sequenceNode.Children.Count)
                    cfg[path] = scalars.Select(MapScalarToValue).ToArray();
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private object? MapScalarToValue(YamlScalarNode scalarNode) {
        if (int.TryParse(scalarNode.Value!, out int i)) {
            return i;
        } else if (double.TryParse(scalarNode.Value!, CultureInfo.InvariantCulture, out double d)) {
            return d;
        } else if (scalarNode.Value?.ToLower() is "true") {
            return true;
        } else if (scalarNode.Value?.ToLower() is "false") {
            return false;
        } else {
            return scalarNode.Value;
        }
    }

}
