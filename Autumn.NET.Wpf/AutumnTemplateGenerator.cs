using System;
using System.Windows.Controls;
using System.Windows;

namespace Autumn.WPF;

public sealed class AutumnTemplateGenerator : ContentControl {

    public delegate object TemplateFactory(Type viewType);

    internal static readonly DependencyProperty FactoryProperty =
            DependencyProperty.Register("Factory", typeof(Func<object>), typeof(AutumnTemplateGenerator), new PropertyMetadata(null, FactoryChanged));

    private static void FactoryChanged(DependencyObject instance, DependencyPropertyChangedEventArgs args) {
        var control = (AutumnTemplateGenerator)instance;
        var factory = (Func<object>)args.NewValue;
        control.Content = factory();
    }

    public static DataTemplate CreateDataTemplate(Type model, Type view, TemplateFactory factory) {
        if (factory == null)
            throw new ArgumentNullException(nameof(factory));

        var frameworkElementFactory = new FrameworkElementFactory(typeof(AutumnTemplateGenerator));
        frameworkElementFactory.SetValue(FactoryProperty, () => {
            return factory(view);
        });

        return new DataTemplate(model) {
            DataType = model,
            VisualTree = frameworkElementFactory
        };

    }

}
