using Autumn.Annotations;

using Autumn.Http.Annotations;
using Autumn.Http.Interceptors;

namespace Autumn.Test.Http.Interceptors;

public class AutumnHttpInterceptorChainTests {

    private int IndexOf(Type type) {
        if (type == typeof(TypeA)) return 0;
        if (type == typeof(TypeB)) return 1;
        if (type == typeof(TypeC)) return 2;
        return -1;
    }

    [Fact]
    public void GetOrder_ReturnsOrderAttributeValue() {
        var result = AutumnHttpInterceptorChain.GetOrder(typeof(TypeWithOrder), 10, IndexOf);
        Assert.Equal(5, result);
    }

    [Fact]
    public void GetOrder_ReturnsBeforeValueMinusOne() {
        var result = AutumnHttpInterceptorChain.GetOrder(typeof(TypeBeforeB), 10, IndexOf);
        Assert.Equal(0, result);
    }

    [Fact]
    public void GetOrder_ReturnsAfterValuePlusOne() {
        var result = AutumnHttpInterceptorChain.GetOrder(typeof(TypeAfterA), 10, IndexOf);
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetOrder_RespectsPrecedence() {
        var result = AutumnHttpInterceptorChain.GetOrder(typeof(TypeAfterC), 10, IndexOf);
        Assert.Equal(-1, result);
    }

    [Fact]
    public void GetOrder_ReturnsLastIndexWhenNoAttributes() {
        var result = AutumnHttpInterceptorChain.GetOrder(typeof(TypeWithoutAttributes), 10, IndexOf);
        Assert.Equal(10, result);
    }

    // Test classes with attributes

    [Order(5)]
    private class TypeWithOrder { }

    [InterceptBefore(typeof(TypeB))]
    private class TypeBeforeB { }

    [InterceptAfter(typeof(TypeA))]
    private class TypeAfterA { }

    [InterceptAfter(typeof(TypeC))]
    [InterceptBefore(typeof(TypeA))]
    private class TypeAfterC { }

    private class TypeWithoutAttributes { }

    private class TypeA { }
    private class TypeB { }
    private class TypeC { }

}
