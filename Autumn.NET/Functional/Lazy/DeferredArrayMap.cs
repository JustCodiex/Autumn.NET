namespace Autumn.Functional.Lazy;

public sealed class DeferredArrayMap<TIn, TOut> : DeferredExecution {
    private readonly Func<TIn, TOut> mapper;
    public DeferredArrayMap(Func<TIn,TOut> mapper) {
        this.mapper = mapper;
    }
    public TOut[] Map(TIn[]  map) {
        TOut[] mapped = new TOut[map.Length];
        for (int i = 0; i < mapped.Length; i++) {
            mapped[i] = mapper(map[i]);
        }
        return mapped;
    }
    public override Array Compute(Array input) {
        if (input is TIn[] tin) {
            return Map(tin);
        }
        throw new Exception();
    }
}
