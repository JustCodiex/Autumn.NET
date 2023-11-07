using System.Diagnostics;
using System.Reflection;

using Autumn.Annotations;
using Autumn.Annotations.Library;
using Autumn.Arguments;
using Autumn.Context;
using Autumn.Http;
using Autumn.Http.Annotations;
using Autumn.Scheduling;

namespace Autumn;

/// <summary>
/// Represents the entry point and application management for the Autumn framework.
/// </summary>
public sealed class AutumnApplication {

    private record ShutdownListener(object Target, MethodInfo Method, ShutdownAttribute ShutdownAttribute);

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
    private AutumnScheduler? scheduler;

    private readonly List<Thread> threads;
    private readonly object? main;
    private readonly List<ShutdownListener> shutdownListeners;

    private AutumnApplication(object? main) {
        this.AppContext = new AutumnAppContext();
        this.threads = new List<Thread>();
        this.shutdownListeners = new List<ShutdownListener>();
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

        // Create context loader
        ContextLoader loader = new ContextLoader();
        var subTypes = loader.GetTypes(mainClassType);

        // Load it up
        loader.LoadAssemblyContext(app.AppContext);
        loader.LoadContext(app.AppContext, subTypes);

        // Parse arguments
        CommandLineArgs cmdArgs = new CommandLineArgs(args);
        if (!cmdArgs.Parse()) {
            // Error?
        }

        // Register app context
        app.AppContext.RegisterComponent(cmdArgs);

        // Create containers
        List<(object, MethodInfo?, EndpointAttribute?)> endpointsServices = new();
        List<(object, MethodInfo?, ScheduledAttribute?)> scheduledServices = new();
        object? entryPoint = mainClass;
        MethodInfo? entryPointMethod = mainClassType.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        // Loop over services
        var services = app.AppContext.GetServices();
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

            var schedules = serviceKlass.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(x => (x, x.GetCustomAttribute<ScheduledAttribute>())) // TODO: Add support for the interface, so custom scheduling implementations are supported
                .Where(x => x.Item2 is not null);
            foreach (var (scheduled, scheduleDesc) in schedules) {
                scheduledServices.Add((service, scheduled, scheduleDesc));
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

            var shutdownHandlerMethods = serviceKlass.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(x => (x, x?.GetCustomAttribute<ShutdownAttribute>()))
                .Where(x => x.Item2 is not null).ToArray();
            foreach (var (shutdown, shutdownDesc) in shutdownHandlerMethods) {
                app.shutdownListeners.Add(new ShutdownListener(service, shutdown, shutdownDesc!));
            }

        }

        // Init main class
        if (mainClass is not null) {
            app.AppContext.InitialiseContextObject(mainClass, mainClassType);
        }

        // Start server if endpoints
        if (endpointsServices.Count > 0) {
            if (!AutumnHttpServer.IsSupported) {
                throw new NotSupportedException("Cannot start an HTTP server on an unsupported system");
            }
            app.httpServer = new AutumnHttpServer(app.AppContext);
            foreach (var endpoints in endpointsServices) {
                app.httpServer.RegisterEndpoint(endpoints.Item1, endpoints.Item2!, endpoints.Item3!);
            }
            app.AppContext.RegisterComponent(app.httpServer); // Make it visible to potential consumers (TODO: Make a method that applies this retroactively)
            Thread httpListenerThread = new Thread(app.httpServer.Start);
            httpListenerThread.Start();
            app.threads.Add(httpListenerThread);
        }

        // Add schedules if any
        if (scheduledServices.Count > 0) {
            app.scheduler = new AutumnScheduler();
            foreach (var scheduledMethod in scheduledServices) {
                app.scheduler.Schedule(scheduledMethod.Item1!, scheduledMethod.Item2!, scheduledMethod.Item3!);
            }
            app.AppContext.RegisterComponent(app.scheduler);
            Thread scheduleListenerThread = new Thread(app.scheduler.Start);
            scheduleListenerThread.Start();
            app.threads.Add(scheduleListenerThread);
        }

        // Get additional application loaders
        var appLoaders = loader.ApplicationLoaders;
        foreach (var appLoader in appLoaders) {
            InvokeAppLoader(mainClass, mainClassType, appLoader.ClassTarget, appLoader.LoaderAttribute, app.AppContext, subTypes);
        }

        // Register hook into ctrl+c
        Console.CancelKeyPress += (sender, e) => {
            e.Cancel = app.ExitApplication();
        };

        if (entryPoint is not null && entryPointMethod is not null) {
            entryPointMethod.Invoke(entryPoint, Array.Empty<object>());
        }

        // Join
        if (!args.Contains("-AnoJoin=true")) {
            app.WaitForExitInternal();
        }

    }

    private static void InvokeAppLoader(object? mainKlass, Type mainKlassType, Type appLoader, AutumnApplicationLoaderAttribute appLoaderConfig, 
        AutumnAppContext appContext, IList<Type> subTypes) {

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

    /// <summary>
    /// Waits for the <see cref="AutumnApplication"/> to exit
    /// </summary>
    public void WaitForExit() {
        BeginShutdown();
        WaitForExitInternal();
        InvokeShutdownListeners(true);
    }

    private void BeginShutdown() {
        httpServer?.Shutdown();
        scheduler?.Shutdown();
    }

    private void WaitForExitInternal() {
        foreach (var thread in threads) {
            try {
                thread.Join();
            } catch { }
        }
    }

    private void InvokeShutdownListeners(bool wasGraceful) {
        foreach (var handler in this.shutdownListeners) {
            if (handler.ShutdownAttribute.GracefulShutdownOnly && !wasGraceful) {
                continue;
            }
            handler.Method.Invoke(handler.Target, Array.Empty<object>());
        }
    }

    private bool ExitApplication() {
        // TODO: Logic here to determine if we should actually exit
        WaitForExit();
        return false;
    }

}
