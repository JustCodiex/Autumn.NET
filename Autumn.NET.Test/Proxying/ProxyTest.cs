using System.Reflection;

using Autumn.Proxying;

namespace Autumn.Test.Proxying;

public class ProxyTest {

    public interface ICalculator {
        double Add(double x, double y);
    }

    class Calculator : ICalculator {
        public double Add(double x, double y) => x + y;
    }

    class Logger(ICalculator calculator) : IProxy {
        public List<string> Args { get; } = [];
        public object? HandleMethod(MethodInfo targetMethod, object?[] arguments) {
            Args.Add($"{targetMethod.Name}({string.Join(',', arguments)})");
            return targetMethod.Invoke(calculator, arguments);
        }
    }

    [Fact]
    public void CanCreateProxy() {

        ICalculator original = new Calculator();
        Logger logger = new Logger(original);
        ICalculator loggingCalculator = Proxy.CreateProxy<ICalculator>(logger);

        double three = loggingCalculator.Add(1, 2);
        Assert.Equal(3, three);

        Assert.Single(logger.Args);
        string log = logger.Args[0];
        Assert.Equal($"{nameof(ICalculator.Add)}(1,2)", log);

    }

    [Fact]
    public void CanCreateFunctionProxy() {

        ICalculator original = new Calculator();
        List<string> args = [];

        ICalculator loggingCalculator = Proxy.CreateProxy<ICalculator>((method, arguments) => {
            args.Add($"{method.Name}({string.Join(',', arguments)})");
            return method.Invoke(original, arguments);
        });

        double three = loggingCalculator.Add(5, 3);
        Assert.Equal(8, three);

        Assert.Single(args);
        string log = args[0];
        Assert.Equal($"{nameof(ICalculator.Add)}(5,3)", log);

    }

}
