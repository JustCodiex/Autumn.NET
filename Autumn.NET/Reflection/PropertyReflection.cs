using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Autumn.Reflection;

/// <summary>
/// Provides reflection-based utilities for working with properties.
/// </summary>
public static class PropertyReflection {

    /// <summary>
    /// Creates a delegate to the getter of the specified property.
    /// </summary>
    /// <typeparam name="TValue">The type of the property's value.</typeparam>
    /// <typeparam name="TSource">The type of the object containing the property.</typeparam>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="bindingFlags">Optional flags that determine the way in which the search for the property is conducted. The default is <see cref="BindingFlags.Instance"/> | <see cref="BindingFlags.Public"/>.</param>
    /// <returns>A delegate that, when invoked, gets the value of the specified property on a given instance of <typeparamref name="TSource"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the specified property does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the property does not have a getter or when its type does not match the expected <typeparamref name="TValue"/>.</exception>
    public static Func<TSource, TValue> Getter<TValue, TSource>(string propertyName, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public) {

        PropertyInfo property = typeof(TSource).GetProperty(propertyName, bindingFlags) 
            ?? throw new ArgumentException($"'{typeof(TSource).Name}' does not have a property named '{propertyName}'.");

        if (property.PropertyType != typeof(TValue))
            throw new InvalidOperationException($"Property '{propertyName}' is of type '{property.PropertyType.Name}', not '{typeof(TValue).Name}'.");

        MethodInfo getter = property.GetGetMethod(true) 
            ?? throw new InvalidOperationException($"Property '{propertyName}' does not have a getter.");

        return (instance => (TValue)getter.Invoke(instance, null)!);

    }

}
