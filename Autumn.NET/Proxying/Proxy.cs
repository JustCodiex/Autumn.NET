using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics.CodeAnalysis;

namespace Autumn.Proxying;

/// <summary>
/// Static utility class for creating proxy instances.
/// </summary>
public static class Proxy {

    private sealed class LambdaProxy<T>(Func<MethodInfo, object?[], object?> handler) : IProxy {
        public object? HandleMethod(MethodInfo targetMethod, object?[] arguments) => handler(targetMethod, arguments);
    }

    private static readonly MethodInfo _proxyHandleMethod = typeof(IProxy).GetMethod(nameof(IProxy.HandleMethod), BindingFlags.Instance | BindingFlags.Public) ?? throw new InvalidProgramException("Expecteded method 'HandleMethod' not found");
    private static ModuleBuilder? _moduleBuilder;
    private static ModuleBuilder ProxyModule {
        get {
            if (_moduleBuilder is not null)
                return _moduleBuilder;
            AssemblyName assemblyName = new AssemblyName("Autumn.Dynamic.DynamicProxyAssembly");
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            _moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            return _moduleBuilder;
        }
    }

    /// <summary>
    /// Create a new concrete instance of <typeparamref name="T"/> routing all calls through the provided <paramref name="proxy"/> instance.
    /// </summary>
    /// <typeparam name="T">The interface or abstract class to create a proxy for.</typeparam>
    /// <param name="proxy">The <see cref="IProxy"/> instance to proxy method calls through.</param>
    /// <returns>A dynamically constructed instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T CreateProxy<T>(IProxy proxy) {

        Type targetType = typeof(T);

        if (!(targetType.IsInterface || targetType.IsAbstract))
            throw new InvalidOperationException($"Invalid type argument T : {typeof(T)} must be an interface or abstract type");

        if (!(targetType.IsPublic || targetType.IsNestedPublic))
            throw new InvalidOperationException("Cannot make a proxy of a non-public type");

        var instance = CreateDynamicObject(targetType, proxy);
        if (instance is T t)
            return t;

        throw new InvalidOperationException($"Unable to make a proxy of type {targetType}");

    }

    /// <summary>
    /// Create a new concrete instance of <typeparamref name="T"/> routing all calls through the provided <paramref name="handler"/> function.
    /// </summary>
    /// <typeparam name="T">The interface or abstract class to create a proxy for.</typeparam>
    /// <param name="handler">The function to proxy method calls through.</param>
    /// <returns>A dynamically constructed instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T CreateProxy<T>(Func<MethodInfo, object?[], object?> handler) {
        var proxy = new LambdaProxy<T>(handler);
        return CreateProxy<T>(proxy);
    }

    private static object? CreateDynamicObject(Type proxyType, IProxy proxy) {

        MethodInfo[] interfaceMethods = proxyType.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        // Define a new type that implements I
        TypeBuilder typeBuilder = ProxyModule.DefineType(proxyType.Name+"@Proxy#"+proxy.GetHashCode(), TypeAttributes.Public);
        typeBuilder.AddInterfaceImplementation(proxyType);

        // Define private proxy field
        FieldBuilder __proxy = typeBuilder.DefineField("__proxy", typeof(IProxy), FieldAttributes.Private);
        FieldBuilder __methods = typeBuilder.DefineField("__methods", typeof(MethodInfo[]), FieldAttributes.Private | FieldAttributes.InitOnly);

        ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, [typeof(IProxy), typeof(MethodInfo[])]);
        ILGenerator constructorCode = constructorBuilder.GetILGenerator();
        constructorCode.Emit(OpCodes.Ldarg_0);
        constructorCode.Emit(OpCodes.Ldarg_1);
        constructorCode.Emit(OpCodes.Stfld, __proxy);
        constructorCode.Emit(OpCodes.Ldarg_0);
        constructorCode.Emit(OpCodes.Ldarg_2);
        constructorCode.Emit(OpCodes.Stfld, __methods);
        constructorCode.Emit(OpCodes.Ret);

        // Implement methods from the interface
        for (int i = 0; i < interfaceMethods.Length; i++) {

            var interfaceMethod = interfaceMethods[i];
            var parameters = interfaceMethod.GetParameters();
            var parameterTypes = parameters.Select(x => x.ParameterType).ToArray();

            // Define the method
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(
                interfaceMethod.Name,
                MethodAttributes.Public | MethodAttributes.Virtual,
                interfaceMethod.ReturnType,
                parameterTypes);

            ILGenerator il = methodBuilder.GetILGenerator();

            // Load the proxy field (__proxy) onto the stack
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldfld, __proxy);

            // Load the MethodInfo corresponding to this method from the __methods array
            il.Emit(OpCodes.Ldarg_0); // Load "this"
            il.Emit(OpCodes.Ldfld, __methods); // Load the MethodInfo[] array
            il.Emit(OpCodes.Ldc_I4, i); // Load the index of the current method
            il.Emit(OpCodes.Ldelem_Ref); // Load the MethodInfo at the index

            // Create an object array to hold method parameters
            il.Emit(OpCodes.Ldc_I4, parameters.Length); // Push the size of the array
            il.Emit(OpCodes.Newarr, typeof(object)); // Create a new object array

            // Load each parameter and box if necessary
            for (int j = 0; j < parameters.Length; j++) {
                il.Emit(OpCodes.Dup); // Duplicate the array reference
                il.Emit(OpCodes.Ldc_I4, j); // Push the index

                il.Emit(OpCodes.Ldarg, j + 1); // Load the method argument (Ldarg_1 is for the first argument, so i+1)

                if (parameters[j].ParameterType.IsValueType) {
                    il.Emit(OpCodes.Box, parameters[j].ParameterType); // Box value types
                }

                il.Emit(OpCodes.Stelem_Ref); // Store the argument in the array
            }

            // Call IProxy.Invoke, passing the method name and arguments
            il.Emit(OpCodes.Callvirt, _proxyHandleMethod);

            // Handle return value
            if (interfaceMethod.ReturnType == typeof(void)) {
                il.Emit(OpCodes.Pop); // Discard the result if the method returns void
            } else if (interfaceMethod.ReturnType.IsValueType) {
                // Unbox value types
                il.Emit(OpCodes.Unbox_Any, interfaceMethod.ReturnType);
            }

            il.Emit(OpCodes.Ret); // Return from the method

            // Implement the method from the interface
            typeBuilder.DefineMethodOverride(methodBuilder, interfaceMethod);
        }

        Type dynamicType = typeBuilder.CreateType();
        object? dynamicInstance = Activator.CreateInstance(dynamicType, [proxy, interfaceMethods]);

        return dynamicInstance;

    }

}
