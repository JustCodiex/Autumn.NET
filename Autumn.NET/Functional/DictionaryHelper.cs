namespace Autumn.Functional;

public static class DictionaryHelper {

    public static TExpectedValue? GetOrElse<Tkey, TValue, TExpectedValue>(this IDictionary<Tkey, TValue?> dictionary, Tkey key, TExpectedValue? defaultValue) {
        if (dictionary.TryGetValue(key, out TValue? value) && value is TExpectedValue expectedValue) {
            return expectedValue;
        }
        return defaultValue;
    }

}
