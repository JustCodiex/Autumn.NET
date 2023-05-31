using Autumn.Annotations;
using Autumn.Http.Annotations;

namespace UnitConverter;

[Service]
public class RestService {

    [Inject]
    public IConverter TemperatureConverter { get; }

    [Endpoint("/temperature")]
    public string ConvertTemperature(string from, float fromValue, string to) {
        return TemperatureConverter.Convert(from, fromValue, to);
    }

}
