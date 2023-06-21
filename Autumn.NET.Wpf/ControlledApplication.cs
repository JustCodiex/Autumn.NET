using System;
using System.IO;
using System.Reflection;
using System.Windows;

using Autumn.Context;
using Autumn.WPF.Annotations;

namespace Autumn.WPF;

public class ControlledApplication : Application {

    public AutumnAppContext? AppContext { get; set; }

    protected override void OnStartup(StartupEventArgs e) {
        
        // Run our application
        AutumnApplication.Run((object)this, e.Args);

        // Ensure appcontext is set
        if (AppContext is null) {
            throw new Exception();
        }

        // Store startup uri
        var startupUri = this.StartupUri;

        // Cancel startup uri
        typeof(Application).GetField("_startupUri", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(this, null);

        // Translate to type and create it
        var windowType = TranslateViewToType(startupUri);
        AppContext.RegisterComponent(windowType);
        CreateWindow(windowType);

    }

    private Type TranslateViewToType(Uri uri) { // TODO: Handle more scenarios

        string domain = this.GetType().Namespace;
        string klass = Path.GetFileNameWithoutExtension(uri.OriginalString);
        string assumedTypename = $"{domain}.{klass}";

        var sourceAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly() ?? Assembly.GetExecutingAssembly() ?? throw new Exception();
        return sourceAssembly.GetType(assumedTypename) ?? throw new Exception();

    }

    private void CreateWindow(Type windowType) {

        var modelType = ModelAttribute.GetModel(windowType);
        object? modelComponent = modelType is null ? null : AppContext!.GetInstanceOf(modelType);

        var windowObject = AutumnWpfApplication.CreateView(windowType, modelComponent, AppContext!);
        if (windowObject is not Window window) {
            throw new Exception();
        }

        this.MainWindow = window;
        this.MainWindow.Show();

    }

}
