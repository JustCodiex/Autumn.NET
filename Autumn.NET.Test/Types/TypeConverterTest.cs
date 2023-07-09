using Autumn.Types;

namespace Autumn.Test.Types;

public class TypeConverterTest {

    [Fact]
    public void CanConvertToStringArray() {

        // Arrange
        object[] arr = { "a", "b", "c" };

        // Act
        object? result = TypeConverter.Convert(arr, typeof(object[]), typeof(string[]));

        // Assert
        Assert.NotNull(result);
        Assert.True(result is string[]);

    }

    [Fact]
    public void CanConvertToIntArray() {
        object[] arr = { 1, 2, 3 };
        object? result = TypeConverter.Convert(arr, typeof(object[]), typeof(int[]));
        Assert.NotNull(result);
        Assert.True(result is int[]);
    }

    [Fact]
    public void CanConvertToDoubleArray() {
        object[] arr = { 1.5, 2.7, 3.9 };
        object? result = TypeConverter.Convert(arr, typeof(object[]), typeof(double[]));
        Assert.NotNull(result);
        Assert.True(result is double[]);
    }

    [Fact]
    public void CanConvertToObjectArray() {
        string[] arr = { "a", "b", "c" };
        object? result = TypeConverter.Convert(arr, typeof(string[]), typeof(object[]));
        Assert.NotNull(result);
        Assert.True(result is object[]);
    }

    [Fact]
    public void CanConvertEmptyArray() {
        object[] arr = Array.Empty<object>();
        object? result = TypeConverter.Convert(arr, typeof(object[]), typeof(string[]));
        Assert.NotNull(result);
        Assert.True(result is string[]);
        Assert.Empty((string[])result);
    }

}
