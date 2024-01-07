namespace Autumn.Context.Factory;

internal interface IComponentFactory {

    void CreateFactory(ComponentIdentifier identifier);

    object GetComponent(ComponentIdentifier identifier, object[] constructorArguments, IScopeContext? scope);

}
