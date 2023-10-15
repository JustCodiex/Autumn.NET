using Autumn.Scheduling;

namespace Autumn.Test.Scheduling;

public class ScheduleTest {

    private record ScheduleTestCase(DateTime Expected, DateTime From) {
        public void Assert(DateTime actual) => Xunit.Assert.Equal(Expected, actual);
    }
    private void AssertAll(Schedule s, params ScheduleTestCase[] tests) => tests.ToList().ForEach(x => x.Assert(s.GetNext(x.From)));

    [Fact]
    public void CanParseScedules() {

        Schedule a = new Schedule("30 0 0 1 * 1"); // The 30th second at the start of the year
        AssertAll(a,
            new ScheduleTestCase(new DateTime(2024, 1, 1, 0, 0, 30), new DateTime(2023, 10, 10, 10, 10, 10)));

        Schedule b = new Schedule("30 * * * * *"); // Every 30th second
        AssertAll(b,
            new ScheduleTestCase(new DateTime(2023, 10, 10, 10, 10, 30), new DateTime(2023, 10, 10, 10, 10, 10)),
            new ScheduleTestCase(new DateTime(2023, 10, 10, 10, 11, 30), new DateTime(2023, 10, 10, 10, 11, 29)),
            new ScheduleTestCase(new DateTime(2023, 10, 10, 10, 12, 30), new DateTime(2023, 10, 10, 10, 11, 31)));

    }

}
