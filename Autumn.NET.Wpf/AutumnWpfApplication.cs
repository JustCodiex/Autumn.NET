using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Autumn.Annotations.Internal;
using Autumn.Annotations.Library;
using Autumn.Context;
using Autumn.WPF.Annotations;

namespace Autumn.WPF;

/// <summary>
/// Represents the entry point for Autumn.NET WPF applications.
/// </summary>
/// <remarks>
/// The <c>AutumnWpfApplication</c> class is responsible for loading and managing Autumn.NET WPF applications.
/// It provides methods for loading the application, retrieving views, and injecting data models.
/// </remarks>
[AutumnApplicationLoader]
public sealed class AutumnWpfApplication {

    private static AutumnWpfApplication? instance;
    private static readonly Dictionary<Dispatcher, AutumnWpfApplication> wpfApps = new Dictionary<Dispatcher, AutumnWpfApplication>();

    public static AutumnWpfApplication Instance => instance ?? throw new Exception();

    private Application? application;

    private AutumnAppContext? appContext;

    private readonly Dictionary<Type, HashSet<Type>> modelViewPairs = new Dictionary<Type, HashSet<Type>>();

    public Application ApplicationTarget => application ?? throw new ApplicationInitializationException("Application target is undefined");

    /// <summary>
    /// Loads the Autumn.NET WPF application with the specified parameters.
    /// </summary>
    /// <param name="main">The main WPF application object.</param>
    /// <param name="appContext">The Autumn application context.</param>
    /// <param name="appTypes">An array of types representing the application types.</param>
    /// <exception cref="ApplicationInitializationException">Thrown when the main object is not a valid WPF application target.</exception>
    public void LoadApplication(object? main, AutumnAppContext appContext, Type[] appTypes) {

        if (main is not ControlledApplication app) {
            throw new ApplicationInitializationException("Invalid WPF application target");
        }

        wpfApps[app.Dispatcher] = this;

        instance = this;
        instance.application = app;
        instance.appContext = appContext;

        ResourceDictionary mvvmBindings = new ResourceDictionary();
        ResourceDictionary converterBindings = new ResourceDictionary();

        for (int i = 0; i < appTypes.Length; i++) {
            if (appTypes[i].GetCustomAttribute<ModelAttribute>() is ModelAttribute modelAttrib) {
                mvvmBindings.Add(modelAttrib.ModelType, AutumnTemplateGenerator.CreateDataTemplate(modelAttrib.ModelType, appTypes[i], t => {
                    var modelInstance = appContext.GetInstanceOf(modelAttrib.ModelType) ?? throw new Exception();
                    var view = CreateView(t, modelInstance, appContext);
                    if (view is Control control) {
                        control.DataContext = modelInstance;
                    }
                    InjectDataModel(view, modelInstance);
                    return view;
                }));
                if (modelViewPairs.TryGetValue(modelAttrib.ModelType, out var views)) {
                    views.Add(appTypes[i]);
                } else {
                    modelViewPairs.Add(modelAttrib.ModelType, new() { appTypes[i] });
                }
            }
            if (appTypes[i].GetCustomAttribute<ValueConverterAttribute>() is ValueConverterAttribute converterAttribute) {
                var converter = appContext.GetInstanceOf(appTypes[i]);
                converterBindings[appTypes[i].Name] = converter;
                converterBindings[appTypes[i]] = converter;
            }
        }

        app.Resources.MergedDictionaries.Add(mvvmBindings);
        app.Resources.MergedDictionaries.Add(converterBindings);
        app.AppContext = appContext;

        // Register standard WPF classes
        app.AppContext.RegisterComponent(app.Dispatcher);

    }

    /// <summary>
    /// Retrieves a view object of the specified type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the view object to retrieve.</typeparam>
    /// <returns>The view object of type <typeparamref name="T"/>.</returns>
    /// <exception cref="ApplicationInitializationException">Thrown if the application or appContext is not properly initialized.</exception>
    /// <exception cref="ViewNotFoundException">Thrown if the view object cannot be found.</exception>
    public static object GetView<T>()
        => GetView(typeof(T));

    public static TView GetView<TModel, TView>() {

        var view = GetView<TModel>();
        if (view is TView actualView) {
            return actualView;
        }

        throw new Exception();

    }

    /// <summary>
    /// Retrieves a view object of the specified type.
    /// </summary>
    /// <returns>The view object.</returns>
    /// <exception cref="ApplicationInitializationException">Thrown if the application or appContext is not properly initialized.</exception>
    /// <exception cref="ViewNotFoundException">Thrown if the view object cannot be found.</exception>
    public static object GetView(Type viewType) {

        if (instance is null) {
            throw new ApplicationInitializationException($"No {nameof(AutumnWpfApplication)} instance found");
        }

        if (instance.application is null) {
            throw new ApplicationInitializationException();
        }

        if (instance.appContext is null) {
            throw new ApplicationInitializationException();
        }

        if (instance.application.TryFindResource(viewType) is not DataTemplate template) {
            throw new ViewNotFoundException();
        }

        var container = template.LoadContent() as AutumnTemplateGenerator ?? throw new ViewNotFoundException();
        var view = container.Content;

        instance.appContext.InitialiseContextObject(view, view.GetType());

        return view;

    }

    public static object GetModelView<T>(T model) where T : class {

        if (instance is null) {
            throw new ApplicationInitializationException($"No {nameof(AutumnWpfApplication)} instance found");
        }

        if (instance.appContext is null) {
            throw new ApplicationInitializationException();
        }

        var t = typeof(T);

        if (!instance.modelViewPairs.TryGetValue(t, out var views)) {
            throw new Exception();
        }

        if (views.Count != 1) {
            throw new Exception();
        }

        var view = CreateView(views.First(), model, instance.appContext);
        if (view is Control control) {
            control.DataContext = model;
        }
        return view;
        
    }

    private static void InjectDataModel(object view, object model) {
        var typ = view.GetType();
        var props = typ.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => (x, x.GetCustomAttribute<DataModelAttribute>()))
            .Where(x => x.Item2 is not null)
            .ToList();
        foreach ( var prop in props) {
            InjectionHelper.Inject(typ, prop.x, view, model);
        }
    }

    internal static object CreateView(Type viewType, object? model, AutumnAppContext appContext) {
        return appContext.CreateContextObject(viewType, (IInjectAnnotation _, ParameterInfo p) => {
            if (p.GetCustomAttribute<DataModelAttribute>() is not null) {
                return (true, model);
            }
            return (false, null);
        }, x => RegisterWindow(x,appContext), Array.Empty<object>()) ?? throw new Exception();
    }

    private static void RegisterWindow(object view, AutumnAppContext appContext) {
        if (view is not Window w) {
            return;
        }
        if (instance is not null && instance.application is not null && instance.application.MainWindow == w) {
            appContext.RegisterComponent(w);
        }
    }

    internal static AutumnWpfApplication GetApplication(object value) {
        if (value is Control control && wpfApps.TryGetValue(control.Dispatcher, out var app)) {
            return app;
        }
        return instance ?? throw new NoApplicationException();
    }

}
