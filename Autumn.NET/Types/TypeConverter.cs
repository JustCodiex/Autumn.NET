using System.Globalization;

namespace Autumn.Types;

/// <summary>
/// Internal utility class for converting between different types.
/// </summary>
internal static class TypeConverter {

    /// <summary>
    /// Converts <paramref name="source"/> from the <paramref name="sourceType"/> to the <paramref name="targetType"/>.
    /// </summary>
    /// <param name="source">The value to convert.</param>
    /// <param name="sourceType">The type of the source value.</param>
    /// <param name="targetType">The target type to convert to.</param>
    /// <returns>The converted value.</returns>
    internal static object? Convert(object? source, Type sourceType, Type targetType) {
        if (sourceType.IsArray) {
            return ConvertArray(source, sourceType.GetElementType() ?? throw new ArgumentException("Unable to determine source element type.", nameof(source)), targetType);
        }

        return System.Convert.ChangeType(source, targetType, CultureInfo.InvariantCulture);

    }

    /// <summary>
    /// Converts an array from the <paramref name="sourceType"/> to the <paramref name="targetType"/>.
    /// </summary>
    /// <param name="source">The array to convert.</param>
    /// <param name="sourceElementType">The element type of the source array.</param>
    /// <param name="targetType">The target array type to convert to.</param>
    /// <returns>The converted array.</returns>
    internal static object? ConvertArray(object? source, Type sourceElementType, Type targetType) {
        if (!targetType.IsArray) {
            throw new ArgumentException("Target type is not an array.", nameof(targetType));
        }
        Array sourceArray = source as Array ?? throw new ArgumentNullException(nameof(source), "Source array is null.");
        Type targetElementType = targetType.GetElementType() ?? throw new ArgumentException("Unable to determine target element type.");
        var arrayInstance = Array.CreateInstance(targetElementType, sourceArray.Length);
        for (int i = 0; i < sourceArray.Length; i++) {
            arrayInstance.SetValue(Convert(sourceArray.GetValue(i), sourceElementType, targetElementType), i);
        }
        return arrayInstance;
    }

}
