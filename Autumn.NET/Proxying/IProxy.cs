using System.Reflection;

namespace Autumn.Proxying;

public interface IProxy {

    object? HandleMethod(MethodInfo targetMethod, object?[] arguments);

}
