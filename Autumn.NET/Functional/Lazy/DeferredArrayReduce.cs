namespace Autumn.Functional.Lazy;

public sealed class DeferredArrayReduce<T> : DeferredExecution {

    private readonly T? initial;
    private readonly Func<T?, T, T?> reducer;

    public DeferredArrayReduce(T? initial, Func<T?, T, T?> reducer) {
        this.initial = initial;
        this.reducer = reducer;
    }

    public override Array Compute(Array input) {
        if (input is T[] tin) {
            T? v = Reduce(tin);
            return new T?[] { v };
        }
        throw new Exception();
    }

    public T? Reduce(T[] input) {
        T? result = initial;
        for (int i = 0; i < input.Length; i++) {
            result = reducer(result, input[i]);
        }
        return result;
    }

}
