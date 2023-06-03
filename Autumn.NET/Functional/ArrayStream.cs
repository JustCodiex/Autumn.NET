using Autumn.Functional.Lazy;

namespace Autumn.Functional;

/// <summary>
/// Represents a streaming instance for arrays.
/// </summary>
public abstract class ArrayStream {

    /// <summary>
    /// The source array for streaming operations.
    /// </summary>
    protected Array source;

    /// <summary>
    /// The list of deferred execution operations to be applied to the source array.
    /// </summary>
    protected LinkedList<DeferredExecution> streamOperations;

    /// <summary>
    /// Initializes a new instance of the ArrayStream class with the specified source array.
    /// </summary>
    /// <param name="array">The source array.</param>
    internal ArrayStream(Array array) {
        this.source = array;
        this.streamOperations = new LinkedList<DeferredExecution>();
    }

    /// <summary>
    /// Initializes a new instance of the ArrayStream class with the specified source array and stream operations.
    /// </summary>
    /// <param name="arraySource">The source array.</param>
    /// <param name="executions">The list of deferred execution operations.</param>
    internal ArrayStream(Array arraySource, LinkedList<DeferredExecution> executions) {
        this.source = arraySource;
        this.streamOperations = new LinkedList<DeferredExecution>(executions);
    }

}

/// <summary>
/// Represents a streaming instance for arrays of a specific type.
/// </summary>
/// <typeparam name="T">The type of elements in the array.</typeparam>
public sealed class ArrayStream<T> : ArrayStream {

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayStream{T}"/> class with the specified source array.
    /// </summary>
    /// <param name="source">The source array.</param>
    public ArrayStream(T[] source) : base(source) {}

    /// <summary>
    /// Initializes a new instance of the <see cref="ArrayStream{T}"/> class with the specified source array stream.
    /// </summary>
    /// <param name="src">The source array stream.</param>
    public ArrayStream(ArrayStream<T> src) : base(src.source, src.streamOperations) { }

    private ArrayStream(LinkedList<DeferredExecution> executions, Array arraySource) : base(arraySource, executions) {}

    /// <summary>
    /// Applies a mapping function to each element in the array stream and returns a new <see cref="ArrayStream{TOut}"/> with the mapped elements.
    /// </summary>
    /// <typeparam name="TOut">The type of the mapped elements.</typeparam>
    /// <param name="func">The mapping function to be applied to each element.</param>
    /// <returns>A new <see cref="ArrayStream{TOut}"/> with the mapped elements.</returns>
    public ArrayStream<TOut> Map<TOut>(Func<T, TOut> func) {
        var next = new ArrayStream<TOut>(this.streamOperations, this.source);
        next.streamOperations.AddLast(new DeferredArrayMap<T, TOut>(func));
        return next;
    }

    /// <summary>
    /// Reduces the elements in the array stream to a single value using the specified reduction function.
    /// </summary>
    /// <param name="func">The reduction function.</param>
    /// <returns>The reduced value.</returns>
    public T? Reduce(Func<T?, T, T?> func) {
        var next = new ArrayStream<T>(this);
        next.streamOperations.AddLast(new DeferredArrayReduce<T>(default, func));
        T[] result = next;
        return result[0];
    }

    /// <summary>
    /// Reduces the elements in the array stream to a single value using the specified reduction function and initial value.
    /// </summary>
    /// <param name="initial">The initial value for the reduction.</param>
    /// <param name="func">The reduction function.</param>
    /// <returns>The reduced value.</returns>
    public T? Reduce(T initial, Func<T?, T, T?> func) {
        var next = new ArrayStream<T>(this);
        next.streamOperations.AddLast(new DeferredArrayReduce<T>(initial, func));
        T[] result = next;
        return result[0];
    }

    /// <summary>
    /// Filters the elements in the array stream based on the specified predicate and returns a new ArrayStream with the filtered elements.
    /// </summary>
    /// <param name="predicate">The predicate function used to filter the elements.</param>
    /// <returns>A new ArrayStream with the filtered elements.</returns>
    public ArrayStream<T> Filter(Predicate<T> predicate) {
        var next = new ArrayStream<T>(this.streamOperations, this.source);
        next.streamOperations.AddLast(new DeferredArrayFilter<T>(predicate));
        return next;
    }

    /// <summary>
    /// Checks if any element in the array stream satisfies the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate function.</param>
    /// <returns>True if any element satisfies the predicate; otherwise, false.</returns>
    public bool Exists(Predicate<T> predicate) {
        var collapsed = Collapse();
        for (int i = 0; i < collapsed.Length; i++) {
            if (predicate(collapsed[i]))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if all elements in the array stream satisfy the specified predicate.
    /// </summary>
    /// <param name="predicate">The predicate function.</param>
    /// <returns>True if all elements satisfy the predicate; otherwise, false.</returns>
    public bool ForAll(Predicate<T> predicate) {
        var collapsed = Collapse();
        for (int i = 0; i < collapsed.Length; i++) {
            if (!predicate(collapsed[i]))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Collapses the array stream into a regular array.
    /// </summary>
    /// <returns>The collapsed array.</returns>
    public T[] Collapse() {
        Array result = this.source;
        var node = this.streamOperations.First;
        while (node is not null) {
            result = node.Value.Compute(result);
            node = node.Next;
        }
        return (T[])result;
    }

    public IDictionary<K, T> ToDictionary<K>(Func<T, K> keymapper) where K : notnull {
        Dictionary<K, T> result = new Dictionary<K, T>();
        var collapsed = this.Collapse();
        for (int i = 0; i < collapsed.Length; i++) {
            result[keymapper(collapsed[i])] = collapsed[i];
        }
        return result;
    }

    /// <summary>
    /// Implicitly converts an ArrayStream to a regular array.
    /// </summary>
    /// <param name="stream">The ArrayStream to convert.</param>
    /// <returns>The collapsed array.</returns>
    public static implicit operator T[](ArrayStream<T> stream) => stream.Collapse();

}

/// <summary>
/// Provides extension methods for converting regular arrays to ArrayStream.
/// </summary>
public static class ArrayStreamInjector {

    /// <summary>
    /// Converts a regular array to an ArrayStream.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="array">The source array.</param>
    /// <returns>An ArrayStream wrapping the source array.</returns>
    public static ArrayStream<T> ToStream<T>(this T[] array) => new ArrayStream<T>(array);

}
