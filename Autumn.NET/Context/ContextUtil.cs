using Autumn.Context.Configuration;

namespace Autumn.Context;

internal static class ContextUtil {

    public static TInterface GetOrCreateComponentFromProperty<TInterface, TDefault>(AutumnAppContext appContext, string property) where TDefault : class, TInterface {
        IConfigSource[] configs = appContext.GetComponents<IConfigSource>();
        if (configs.Length == 0) {
            return appContext.GetInstanceOf<TDefault>();
        }
        return GetOrCreateComponentFromProperty<TInterface, TDefault>(appContext, configs[0], property);
    }

    public static TInterface GetOrCreateComponentFromProperty<TInterface, TDefault>(AutumnAppContext appContext, IConfigSource configSource, string property) where TDefault : class, TInterface {
        string targetType = configSource.GetValueOrDefault(property, typeof(TDefault).FullName) ?? string.Empty;
        if (appContext.GetComponentType(targetType) is not Type componentType) {
            return appContext.GetInstanceOf<TDefault>();
        }
        if (appContext.GetInstanceOf(componentType) is TInterface inf) {
            return inf;
        }
        return appContext.GetInstanceOf<TDefault>();
    }

}
