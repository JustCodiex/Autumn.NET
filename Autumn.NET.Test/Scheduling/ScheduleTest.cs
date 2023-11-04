using Autumn.Scheduling;

namespace Autumn.Test.Scheduling;

public class ScheduleTest {

    private record ScheduleTestCase(DateTime Expected, DateTime From) {
        public void Assert(DateTime actual) => Xunit.Assert.Equal(Expected, actual);
    }
    private void AssertAll(Schedule s, params ScheduleTestCase[] tests) => tests.ToList().ForEach(x => x.Assert(s.GetNext(x.From)));

    [Fact]
    public void CanParseScedules() {
        Assert.Multiple(() => {
            _ = new Schedule("30 0 0 1 * 1"); // The 30th second at the start of the year
            _ = new Schedule("30 * * * * *"); // Every 30th second
            _ = new Schedule("20,40 * * * * *"); // Every 20th or 40th second
        });
    }

    [Fact]
    public void WillGetCorrectNext() {

        Schedule a = new Schedule("30 0 0 1 * 1"); // The 30th second at the start of the year
        AssertAll(a,
            new ScheduleTestCase(new DateTime(2024, 1, 1, 0, 0, 30), new DateTime(2023, 10, 10, 10, 10, 10)));

        Schedule b = new Schedule("30 * * * * *"); // Every 30th second
        AssertAll(b,
            new ScheduleTestCase(new DateTime(2023, 10, 10, 10, 10, 30), new DateTime(2023, 10, 10, 10, 10, 10)),
            new ScheduleTestCase(new DateTime(2023, 10, 10, 10, 11, 30), new DateTime(2023, 10, 10, 10, 11, 29)),
            new ScheduleTestCase(new DateTime(2023, 10, 10, 10, 12, 30), new DateTime(2023, 10, 10, 10, 11, 31)));

        Schedule c = new Schedule("20,40 * * * * *"); // Every 20th or 40th second
        AssertAll(c,
            new ScheduleTestCase(new DateTime(2023, 10, 10, 10, 10, 20), new DateTime(2023, 10, 10, 10, 10, 10)),
            new ScheduleTestCase(new DateTime(2023, 10, 10, 10, 10, 20), new DateTime(2023, 10, 10, 10, 10, 19)),
            new ScheduleTestCase(new DateTime(2023, 10, 10, 10, 11, 20), new DateTime(2023, 10, 10, 10, 10, 45)),
            new ScheduleTestCase(new DateTime(2023, 10, 10, 10, 10, 40), new DateTime(2023, 10, 10, 10, 10, 21)));

        Schedule d = new Schedule("0 */10 * * * *"); // At every 10th minute
        AssertAll(d,
            new ScheduleTestCase(new DateTime(2023, 11, 4, 19, 10, 0), new DateTime(2023, 11, 4, 19, 4, 0)),
            new ScheduleTestCase(new DateTime(2023, 11, 4, 19, 20, 0), new DateTime(2023, 11, 4, 19, 10, 2)),
            new ScheduleTestCase(new DateTime(2023, 11, 4, 20, 0, 0), new DateTime(2023, 11, 4, 19, 58, 40)));

    }

}
