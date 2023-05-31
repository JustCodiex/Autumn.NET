using Autumn.Annotations;

namespace UnitConverter.Converters;

[Component]
public class TemperatureConverter : IConverter {

    private const float C_TO_K_CONSTANT = 273.15f;

    public TemperatureConverter() {
        Console.WriteLine("Created temperature converter");
    }

    [PostProcessor(nameof(Printer)), ExceptionHandler(nameof(OnError))]
    public string Convert(string from, float fromValue, string to) => (from, to) switch {
        ("c", "k") => (fromValue + C_TO_K_CONSTANT).ToString(),
        _ => throw new ConverterException(),
    };

    public void Printer(string value) {
        Console.WriteLine(value);
    }

    public string OnError(Exception ex) {
        Console.WriteLine(ex);
        return "0";
    }

}
