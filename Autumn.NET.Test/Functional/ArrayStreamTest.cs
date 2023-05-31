using Autumn.Functional;

namespace Autumn.Test.Functional;

public class ArrayStreamTest {

    [Fact]
    public void CanMap() {

        int[] ints = { 1, 2, 3, 4, 5, 6 };

        // Map
        int[] mapped = ints.ToStream().Map(x => x * 2);

        // Assert equals
        Assert.Equal(2, mapped[0]);
        Assert.Equal(4, mapped[1]);
        Assert.Equal(6, mapped[2]);
        Assert.Equal(8, mapped[3]);
        Assert.Equal(10, mapped[4]);
        Assert.Equal(12, mapped[5]);

    }

    [Fact]
    public void CanMapToNewType() {

        int[] ints = { 1, 2, 3, 4, 5, 6 };

        // Map
        float[] mapped = ints.ToStream().Map(x => x * 2).Map(x => x * 2.5f);

        // Assert equals
        Assert.Equal(5.0f, mapped[0]);
        Assert.Equal(10.0f, mapped[1]);
        Assert.Equal(15.0f, mapped[2]);
        Assert.Equal(20.0f, mapped[3]);
        Assert.Equal(25.0f, mapped[4]);
        Assert.Equal(30.0f, mapped[5]);

    }

    [Fact]
    public void CanReduce() {

        int[] ints = { 1, 2, 3, 4, 5, 6 };

        Assert.Equal(21, ints.ToStream().Reduce((x, y) => x + y));

    }

    [Fact]
    public void CanReduceWithInitialValue() {

        int[] ints = { 1, 2, 3, 4, 5, 6 };

        Assert.Equal(22, ints.ToStream().Reduce(1, (x, y) => x + y));

    }

    [Fact]
    public void CanMapReduce() {

        int[] ints = { 1, 2, 3, 4, 5, 6 };

        Assert.Equal(42, ints.ToStream().Map(x => x * 2).Reduce((x, y) => x + y));

    }

    [Fact]
    public void CanPickEven() {

        int[] ints = { 1, 2, 3, 4, 5, 6 };

        var evens = ints.ToStream().Filter(x => x % 2 == 0).Collapse();

        Assert.All(evens, x => Assert.True(x % 2 == 0));

    }

}
