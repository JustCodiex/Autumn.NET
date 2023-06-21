using System.Reflection;

namespace Autumn.Context;

internal static class InjectionHelper {

    internal static void Inject(Type klass, PropertyInfo propertyInfo, object? instance, object? dependency) {
        if (propertyInfo.SetMethod is MethodInfo setter) {
            setter.Invoke(instance, new[] { dependency });
        } else {
            var fields = klass.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            if (fields.FirstOrDefault(x => x.Name == $"<{propertyInfo.Name}>k__BackingField") is FieldInfo field) {
                field.SetValue(instance, dependency);
            }
        }
    }

}
