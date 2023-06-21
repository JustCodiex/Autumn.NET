using System;
using System.Windows.Markup;
using System.Xaml;

namespace Autumn.WPF.Extensions;

[MarkupExtensionReturnType(typeof(object))]
public class NamedResource : MarkupExtension {

    private object? name;

    [ConstructorArgument("name")]
    public object? Name {
        get => name;
        set => name = value;
    }

    public NamedResource() { }

    public NamedResource(object name) {
        this.name = name;
    }

    public override object? ProvideValue(IServiceProvider serviceProvider) {
        if (serviceProvider is null) {
            return null;
        }

        IRootObjectProvider rootProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider ?? throw new Exception();
        var root = rootProvider.RootObject;

        var app = AutumnWpfApplication.GetApplication(root);
        if (app.ApplicationTarget.TryFindResource(this.name) is object obj) {
            return obj;
        }

       throw new Exception();

    }

}
