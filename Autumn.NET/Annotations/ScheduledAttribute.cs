using Autumn.Scheduling;

namespace Autumn.Annotations;

/// <summary>
/// Specifies that a method is scheduled to run according to a cron-like schedule.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ScheduledAttribute : Attribute, ISchedulerAttribute {

    private readonly string cronSchedule;
    private Schedule? schedule;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledAttribute"/> class with a cron expression.
    /// </summary>
    /// <param name="cronSchedule">The cron expression that represents the schedule.</param>
    public ScheduledAttribute(string cronSchedule) {
        this.cronSchedule = cronSchedule;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledAttribute"/> class with specific scheduling values. 
    /// Null values indicate that any value is permissible for the corresponding time field.
    /// </summary>
    /// <param name="second">The second when the method should run, or null for any second.</param>
    /// <param name="minute">The minute when the method should run, or null for any minute.</param>
    /// <param name="hour">The hour when the method should run, or null for any hour.</param>
    /// <param name="dayOfWeek">The day of the week when the method should run represented by its three-letter abbreviation, or null for any day.</param>
    /// <param name="dayOfMonth">The day of the month when the method should run, or null for any day of the month.</param>
    /// <param name="month">The month when the method should run, or null for any month.</param>
    public ScheduledAttribute(int? second, int? minute, int? hour, DayOfWeek? dayOfWeek, int? dayOfMonth, int? month) 
        : this(
              second.HasValue ? second.Value.ToString() : "*",
              minute.HasValue ? minute.Value.ToString() : "*",
              hour.HasValue ? hour.Value.ToString() : "*",
              dayOfWeek switch {
                  DayOfWeek.Sunday => "sun",
                  DayOfWeek.Monday => "mon",
                  DayOfWeek.Tuesday => "tue",
                  DayOfWeek.Wednesday => "wed",
                  DayOfWeek.Thursday => "thu",
                  DayOfWeek.Friday => "fri",
                  DayOfWeek.Saturday => "sat",
                  _ => "*"
              },
              dayOfMonth.HasValue ? dayOfMonth.Value.ToString() : "*",
              month.HasValue ? month.Value.ToString() : "*"
              ) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledAttribute"/> class with specific scheduling string values.
    /// </summary>
    /// <param name="seconds">The seconds when the method should run, or a cron-style specification.</param>
    /// <param name="minutes">The minutes when the method should run, or a cron-style specification.</param>
    /// <param name="hours">The hours when the method should run, or a cron-style specification.</param>
    /// <param name="weekDays">The days of the week when the method should run, or a cron-style specification.</param>
    /// <param name="daysOfMonths">The days of the month when the method should run, or a cron-style specification.</param>
    /// <param name="months">The months when the method should run, or a cron-style specification.</param>
    public ScheduledAttribute(string seconds, string minutes, string hours, string weekDays, string daysOfMonths, string months) {
        this.cronSchedule = $"{seconds} {minutes} {hours} {daysOfMonths} {weekDays} {months}";
    }

    /// <summary>
    /// Retrieves the <see cref="Schedule"/> represented by this attribute.
    /// </summary>
    /// <returns>The <see cref="Schedule"/> based on the cron expression.</returns>
    public ISchedule GetSchedule() {
        if (schedule is not null) {
            return schedule;
        }
        schedule = new Schedule(cronSchedule);
        return schedule;
    }

}
