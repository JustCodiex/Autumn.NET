using System.Reflection;

using Autumn.Annotations;
using Autumn.Types;

namespace Autumn.Reflection;

/// <summary>
/// Provides functionality to dynamically construct complex objects and arrays based on a specified type and a set of values.
/// </summary>
public static class ComplexObjectBuilder {

    /// <summary>
    /// Constructs an object of the specified type using the provided values.
    /// </summary>
    /// <param name="expectedType">The type of the object to construct.</param>
    /// <param name="values">A dictionary mapping property names to their values.</param>
    /// <returns>An object of the specified type, constructed using the provided values.</returns>
    public static object? BuildObject(Type expectedType, IDictionary<string, object?> values) {
        if (expectedType.IsArray) {
            return BuildArray(expectedType, values);
        }
        var ctors = expectedType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
        if (ctors.Length == 1) {
            ConstructorInfo ctor = ctors[0];
            ParameterInfo[] parameters = ctor.GetParameters();
            object?[] constructorArgs = new object?[parameters.Length];
            for (int i = 0; i < parameters.Length; i++) {
                constructorArgs[i] = MatchParameter(parameters[i], values);
            }
            object? instance = ctor.Invoke(constructorArgs);
            // TODO: Set property/field values
            return instance;
        }
        return null;
    }

    /// <summary>
    /// Matches and prepares a constructor parameter based on the provided values.
    /// </summary>
    /// <param name="parameter">The parameter information.</param>
    /// <param name="values">A dictionary mapping property names to their values.</param>
    /// <returns>The matched and prepared value for the constructor parameter.</returns>
    private static object? MatchParameter(ParameterInfo parameter, IDictionary<string, object?> values) {
        if (parameter.GetCustomAttribute<ConfigNameAttribute>() is ConfigNameAttribute cfgName && values.TryGetValue(cfgName.Name, out object? value)) {
            return SelfOrConstruct(parameter.ParameterType, value);
        }
        if (values.TryGetValue(parameter.Name!, out value)) {
            return SelfOrConstruct(parameter.ParameterType, value);
        } else if (values.TryGetValue(parameter.Name!.ToLowerInvariant(), out value)) {
            return SelfOrConstruct(parameter.ParameterType, value);
        }
        return value;
    }

    /// <summary>
    /// Constructs an object or returns the provided value if it matches the expected type.
    /// </summary>
    /// <param name="expected">The expected type of the object.</param>
    /// <param name="value">The value to be used or converted.</param>
    /// <returns>An object of the expected type, or the original value if it matches the expected type.</returns>
    private static object? SelfOrConstruct(Type expected, object? value) {
        if (value is IDictionary<string, object?> dictionary) {
            return BuildObject(expected, dictionary);
        }
        if (value is null) {
            return value;
        }
        return TypeConverter.Convert(value, value.GetType(), expected);
    }

    /// <summary>
    /// Constructs an array of the specified type using the provided values.
    /// </summary>
    /// <param name="expectedType">The type of the array to construct.</param>
    /// <param name="values">A dictionary mapping array indices to their values.</param>
    /// <returns>An array of the specified type, constructed using the provided values.</returns>
    private static object? BuildArray(Type expectedType, IDictionary<string, object?> values) {
        Type elementType = expectedType.GetElementType()!;
        if (values.TryGetValue("#", out object? countObj) && countObj is int count) {
            Array array = Array.CreateInstance(elementType, count);
            for (int i = 0; i < count; i++) {
                object? entry = BuildObject(elementType, GetConfigArrayEntry(i, values));
                array.SetValue(entry, i);
            }
            return array;
        }
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves values for an array entry from the provided dictionary.
    /// </summary>
    /// <param name="index">The index of the array entry.</param>
    /// <param name="values">A dictionary containing the values for the array entries.</param>
    /// <returns>A dictionary containing the values for the specified array entry.</returns>
    private static IDictionary<string, object?> GetConfigArrayEntry(int index, IDictionary<string, object?> values) {
        string key = (index+1).ToString();
        return values.Where(x => x.Key.StartsWith(key + ".")).Select(x => (Key: x.Key[(key.Length + 1)..], x.Value))
            .ToDictionary(x => x.Key, x => x.Value);
    }

}
