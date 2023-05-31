namespace Autumn.Context;

internal record struct ComponentIdentifier(string ComponentQualifier, Type ComponentInstanceType) {
    public static ComponentIdentifier DefaultIdentifier(Type instanceType) => new ComponentIdentifier(instanceType.FullName!, instanceType);
}
