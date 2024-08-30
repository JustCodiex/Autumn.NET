using System.Reflection;

namespace Autumn.Reflection;

internal abstract record ObjectInstantiator {
    public abstract object? Instantiate(object?[]? args);
}

internal sealed record ConstructorInstantiator(ConstructorInfo ConstructorInfo) : ObjectInstantiator {
    public override object? Instantiate(object?[]? args) {
        return ConstructorInfo.Invoke(args);
    }
}

internal sealed record FactoryInstantiator(object? FactoryInstance, MethodInfo MethodInfo) : ObjectInstantiator {
    public override object? Instantiate(object?[]? args) {
        return MethodInfo.Invoke(FactoryInstance, args);
    }
}
