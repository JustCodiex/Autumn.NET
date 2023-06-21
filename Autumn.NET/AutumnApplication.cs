using System.Diagnostics;
using System.Reflection;

using Autumn.Annotations;
using Autumn.Annotations.Library;
using Autumn.Arguments;
using Autumn.Context;
using Autumn.Http;
using Autumn.Http.Annotations;

namespace Autumn;

/// <summary>
/// Represents the entry point and application management for the Autumn framework.
/// </summary>
public sealed class AutumnApplication {

    private static AutumnApplication? mainApp;
    private static readonly List<AutumnApplication> activeApps = new();

    /// <summary>
    /// Gets the main application instance.
    /// </summary>
    public static AutumnApplication Application => mainApp ?? throw new NoApplicationException();

    /// <summary>
    /// Gets the application context associated with the current application instance.
    /// </summary>
    public AutumnAppContext AppContext { get; }

    private AutumnHttpServer? httpServer;
    private readonly List<Thread> threads;
    private readonly object? main;

    private AutumnApplication(object? main) {
        this.AppContext = new AutumnAppContext();
        this.threads = new List<Thread>();
        this.main = main;
    }

    /// <summary>
    /// Runs the Autumn application using the entry point determined by the caller type.
    /// </summary>
    /// <param name="args">The command-line arguments passed to the application.</param>
    public static void Run(params string[] args) {

        // Get caller type
        StackFrame frame = new StackFrame(1);
        Type? declaringType = (frame.GetMethod()?.DeclaringType) ?? throw new InvalidProgramException();

        // Invoke private member
        RunInternal(null, declaringType, args);

    }

    /// <summary>
    /// Runs the Autumn application using the specified entry point type.
    /// </summary>
    /// <param name="mainType">The type representing the entry point of the application.</param>
    /// <param name="args">The command-line arguments passed to the application.</param>
    public static void Run(Type mainType, params string[] args)
        => RunInternal(null, mainType, args);

    /// <summary>
    /// Runs the Autumn application using the specified entry point type.
    /// </summary>
    /// <typeparam name="T">The type representing the entry point of the application.</typeparam>
    /// <param name="args">The command-line arguments passed to the application.</param>
    public static void Run<T>(params string[] args) 
        => RunInternal(null, typeof(T), args);

    /// <summary>
    /// Runs the Autumn application using the specified entry point instance.
    /// </summary>
    /// <typeparam name="T">The type representing the entry point of the application.</typeparam>
    /// <param name="main">The instance representing the entry point of the application.</param>
    /// <param name="args">The command-line arguments passed to the application.</param>
    public static void Run<T>(T main, params string[] args) where T : class
        => RunInternal(main, typeof(T), args);

    /// <summary>
    /// Runs the Autumn application using the specified entry point instance.
    /// </summary>
    /// <param name="main">The instance representing the entry point of the application.</param>
    /// <param name="args">The command-line arguments passed to the application.</param>
    public static void Run(object main, params string[] args) 
        => RunInternal(main, main.GetType(), args);

    private static void RunInternal(object? mainClass, Type mainClassType, params string[] args) {

        // Create app
        AutumnApplication app = new AutumnApplication(mainClass);
        activeApps.Add(app);
        mainApp ??= app;

        // Get namespace
        var subTypes = mainClassType.Assembly.GetTypes();
        if (!string.IsNullOrEmpty(mainClassType.Namespace)) {
            subTypes = subTypes.Where(x => x.Namespace?.StartsWith(mainClassType.Namespace) ?? false).ToArray();
        }

        // Load it up
        ContextLoader loader = new ContextLoader();
        loader.LoadAssemblyContext();
        loader.LoadContext(app.AppContext, subTypes);

        // Parse arguments
        CommandLineArgs cmdArgs = new CommandLineArgs(args);
        if (!cmdArgs.Parse()) {
            // Error?
        }

        // Register app context
        app.AppContext.RegisterComponent(cmdArgs);

        // Create services
        var services = app.AppContext.GetServices();
        List<(object, MethodInfo?, EndpointAttribute?)> endpointsServices = new();
        object? entryPoint = mainClass;
        MethodInfo? entryPointMethod = mainClassType.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (var serviceKlass in services) {
            var service = app.AppContext.GetInstanceOf(serviceKlass) ?? throw new Exception();
            var starters = serviceKlass.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(x => (x, x.GetCustomAttribute<StartAttribute>()))
                .Where(x => x.Item2 is not null);
            foreach (var (start, _) in starters) {
                start.Invoke(service, Array.Empty<object>());
            }
            var endpoints = serviceKlass.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(x => (x, x.GetCustomAttribute<EndpointAttribute>()))
                .Where(x => x.Item2 is not null);
            foreach (var (endpoint, endpointDesc) in endpoints) {
                endpointsServices.Add((service, endpoint, endpointDesc));
            }
            var entryPointMethods = serviceKlass.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(x => (x, x?.GetCustomAttribute<EntryPointAttribute>()))
                .Where(x => x.Item2 is not null).ToArray();
            if (entryPointMethods.Length == 1) {
                if (entryPointMethod is not null) {
                    throw new MultipleEntryPointsException("Multiple entry points detected!");
                }
                entryPoint = service;
                entryPointMethod = entryPointMethods[0].x;
            }
        }

        // Init main class
        if (mainClass is not null) {
            app.AppContext.InitialiseContextObject(mainClass, mainClassType);
        }

        // Start server if endpoints
        if (endpointsServices.Count > 0) {
            app.httpServer = new AutumnHttpServer(app.AppContext);
            foreach (var endpoints in endpointsServices) {
                app.httpServer.RegisterEndpoint(endpoints.Item1, endpoints.Item2!, endpoints.Item3!);
            }
            app.AppContext.RegisterComponent(app.httpServer); // Make it visible to potential consumers (TODO: Make a method that applies this retroactively)
            Thread httpListenerThread = new Thread(app.httpServer.Start);
            httpListenerThread.Start();
            app.threads.Add(httpListenerThread);
        }

        // Get additional application loaders
        var appLoaders = loader.ApplicationLoaders;
        foreach (var appLoader in appLoaders) {
            InvokeAppLoader(mainClass, mainClassType, appLoader.ClassTarget, appLoader.LoaderAttribute, app.AppContext, subTypes);
        }

        // Register hook into ctrl+c
        //AppDomain.CurrentDomain.

        if (entryPoint is not null && entryPointMethod is not null) {
            entryPointMethod.Invoke(entryPoint, Array.Empty<object>());
        }

        // Join
        foreach (var thread in app.threads) {
            try {
                thread.Join();
            } catch { }
        }

    }

    private static void InvokeAppLoader(object? mainKlass, Type mainKlassType, Type appLoader, AutumnApplicationLoaderAttribute appLoaderConfig, 
        AutumnAppContext appContext, Type[] subTypes) {

        var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type[] parameters = { typeof(object), typeof(AutumnAppContext), typeof(Type[]) };
        var loader = appLoader.GetMethod("LoadApplication", flags, parameters);
        if (loader is null) {
            flags |= BindingFlags.Static;
            loader = appLoader.GetMethod("LoadApplication", flags, parameters);
            if (loader is null) {
                return;
            }
            loader.Invoke(null, new object[] { mainKlassType, appContext, subTypes });
        } else {
            var instance = Activator.CreateInstance(appLoader) ?? throw new Exception();
            appContext.InitialiseContextObject(instance, appLoader);
            loader.Invoke(instance, new[] { mainKlass, appContext, subTypes });
        }

    }

}
