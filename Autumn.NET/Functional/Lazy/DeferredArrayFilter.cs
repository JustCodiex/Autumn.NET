namespace Autumn.Functional.Lazy;

public sealed class DeferredArrayFilter<T> : DeferredExecution {

    private readonly Predicate<T> predicate;

    public DeferredArrayFilter(Predicate<T> predicate) {
        this.predicate = predicate;
    }

    public T[] Filter(T[] input) {
        T[] subSelection = new T[input.Length];
        int j = 0;
        for (int i = 0; i < input.Length; i++) {
            if (predicate(input[i]))
                subSelection[j++] = input[i];
        }
        return subSelection[..j];
    }

    public override Array Compute(Array input) {
        if (input is T[] tin) {
            return Filter(tin);
        }
        throw new Exception();
    }

}
