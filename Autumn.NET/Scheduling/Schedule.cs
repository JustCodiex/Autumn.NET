﻿using System.Diagnostics.CodeAnalysis;

using Autumn.Reflection;

namespace Autumn.Scheduling;

/// <summary>
/// Provides an implementation for the <see cref="ISchedule"/> interface using cron-like expressions.
/// </summary>
public sealed class Schedule : ISchedule {

    private readonly struct Field(IList<int> values) {
        public int[] Values { get; } = [.. values];
        public ISet<int> ValuesSet { get; } = new HashSet<int>(values);
    }

    //private static readonly Field EverySecond = new Field(Enumerable.Range(0, 59).ToArray());
    private static readonly Field EveryMinute = new Field(Enumerable.Range(0, 59).ToArray());
    private static readonly Field EveryHour = new Field(Enumerable.Range(0, 23).ToArray());
    private static readonly Field EveryDayOfMonth = new Field(Enumerable.Range(1, 31).ToArray());
    private static readonly Field EveryDayOfWeek = new Field(Enumerable.Range(0, 7).ToArray());
    private static readonly Field EveryMonth = new Field(Enumerable.Range(1, 12).ToArray());

    private static readonly Func<DateTime, int> SecondsGetter = PropertyReflection.Getter<int, DateTime>(nameof(DateTime.Second));
    private static readonly Func<DateTime, int> MinutesGetter = PropertyReflection.Getter<int, DateTime>(nameof(DateTime.Minute));
    private static readonly Func<DateTime, int> HoursGetter = PropertyReflection.Getter<int, DateTime>(nameof(DateTime.Hour));

    private readonly Field seconds;
    private readonly Field minutes;
    private readonly Field hours;
    private readonly Field dayOfWeek;
    private readonly Field dayOfMonth;
    private readonly Field month;

    /// <summary>
    /// Initializes a new instance of the <see cref="Schedule"/> class using a cron-like expression.
    /// </summary>
    /// <param name="expression">The cron-like scheduling expression.</param>
    /// <exception cref="ArgumentException">Thrown when the provided expression is malformed.</exception>
    public Schedule(string expression) {

        string[] fields = expression.Split(' ');
        if (fields.Length != 6) {
            throw new ArgumentException($"Malformed schedule expression - expected 6 fields but found {fields.Length}", nameof(expression));
        }

        seconds = ParseField(fields[0], 0, 59);
        minutes = ParseField(fields[1], 0, 59);
        hours = ParseField(fields[2], 0, 23);
        dayOfMonth = ParseField(fields[3], 1, 31);
        dayOfWeek = ParseField(fields[4], 0, 7,
            ("mon", 0),
            ("tue", 1),
            ("wed", 2),
            ("thu", 3),
            ("fri", 4),
            ("sat", 5),
            ("sun", 6));
        month = ParseField(fields[5], 1, 12,
            ("jan", 1),
            ("feb", 2),
            ("mar", 3),
            ("apr", 4),
            ("may", 5),
            ("jun", 6),
            ("jul", 7),
            ("aug", 8),
            ("sep", 9),
            ("oct", 10),
            ("nov", 11),
            ("dec", 12));

    }

    private Schedule(Field seconds, Field minutes, Field hours, Field dayOfMonth, Field dayOfWeek, Field month) {
        this.seconds = seconds;
        this.minutes = minutes;
        this.hours = hours;
        this.dayOfMonth = dayOfMonth;
        this.dayOfWeek = dayOfWeek;
        this.month = month;
    }

    private static Field ParseField(string field, int min, int max, params (string,int)[] valuemaps) {
        List<int> entries = [];
        if (field == "*") {
            return new Field(Enumerable.Range(min, (max-min)+1).ToArray());
        }
        string[] segments = field.Split(',');
        for (int i = 0; i < segments.Length; i++) {
            if (segments.Length > 1 && string.IsNullOrEmpty(segments[i])) { continue; }
            var valuemapped = ParseFieldValues(segments[i], valuemaps);
            if (valuemapped is int valueentry) {
                entries.Add(valueentry);
                continue;
            }
            int step = segments[i].IndexOf('/');
            if (step != -1) {
                string first = segments[i][0..step];
                string second = segments[i][(step+1)..];
                entries.AddRange(ParseEveryNth(first, second, min, max));
                continue;
            }
            // TODO: More parsing here
            if (!int.TryParse(segments[i], out var value)) {
                throw new ArgumentException("Unable to parse field "+field, nameof(field));
            }
            if (value < min) {
                throw new ArgumentOutOfRangeException(nameof(field), "Value cannot be less than "+min);
            }
            if (value > max) {
                throw new ArgumentOutOfRangeException(nameof(field), "Value cannot be greater than " + max);
            }
            entries.Add(value);
        }
        entries.Sort();
        return new Field(entries);
    }

    private static IEnumerable<int> ParseEveryNth(string first, string second, int min, int max) {
        if (!int.TryParse(second, out int step)) {
            throw new ArgumentException("Unable to parse field " + first + "/" + second + " since " + second + " is not a valid value");
        }
        if (first != "*") {
            if (int.TryParse(first, out var value) && value < max) {
                min = value;
            } else {
                throw new ArgumentOutOfRangeException(nameof(first));
            }
        }
        return Enumerable.Range(min, (max-min)+1).Where(x => x % step == 0);
    }

    private static int? ParseFieldValues(string field, (string, int)[] valuemaps) {
        for (int j = 0; j < valuemaps.Length; j++) {
            if (valuemaps[j].Item1.Equals(field, StringComparison.CurrentCultureIgnoreCase)) {
                return valuemaps[j].Item2;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the next scheduled <see cref="DateTime"/> after the provided date and time.
    /// </summary>
    /// <param name="from">The date and time to start the search from.</param>
    /// <returns>The next scheduled date and time after the provided one.</returns>
    public DateTime GetNext(DateTime from)
        => GetNextInternal(from, DateTime.MaxValue);

    private DateTime GetNextInternal(DateTime from, DateTime stopAfter) {

        int debugIterations = 0;

        DateTime t0 = from.AddSeconds(1); // Always advance by one second since we're asking for the next available (and we might be asking when from would match our predicate)
        while (!IsScheduledTime(t0)) {
            var t1 = NextTimeUnitIfApplicable(t0, seconds, SecondsGetter, (x, y) => x.AddSeconds(y), 60); // Next second
            var t2 = NextTimeUnitIfApplicable(t1, minutes, MinutesGetter, (x, y) => x.AddMinutes(y), 60); // Next minute
            var t3 = NextTimeUnitIfApplicable(t2, hours, HoursGetter, (x, y) => x.AddHours(y), 24/*, 1*/); // Next hour
            var t4 = NextDay(t3); // Next day
            var t5 = NextMonth(t4); // Next month
            // TODO: Handle othe time units
            debugIterations++;
            t0 = t5;
            if (t0 >= stopAfter) {
                return stopAfter;
            }
        }

        return t0;

    }

    private static DateTime NextTimeUnitIfApplicable(DateTime t, Field field, Func<DateTime, int> getter, Func<DateTime, double, DateTime> adder, int resetValue, int offset = 0) {
        int current = getter(t);
        int u = FirstGreaterThan(current, field.Values);
        if (u < 0) {
            return adder(adder(t, -current), resetValue+ offset);
        }
        return adder(t, u - current);
    }

    private DateTime NextDay(DateTime t) {
        int d = t.Day;
        int nextDayInMonth = FirstGreaterThan(d, dayOfMonth.Values);
        if (nextDayInMonth < 0) {
            var t1 = t.AddDays(-d+1);
            return t1.AddMonths(1);
        }
        var t2 = t.AddDays(nextDayInMonth - d);
        return t2;
    }

    private DateTime NextMonth(DateTime t) {
        int m = t.Month;
        int nextMonth = FirstGreaterThan(m, month.Values);
        if (nextMonth < 0) {
            var t1 = t.AddMonths(-m+1);
            return t1.AddYears(1);
        }
        var t2 = t.AddMonths(nextMonth - m);
        return t2;
    }

    private static int FirstGreaterThan(int k, int[] values) {
        int i = Array.BinarySearch(values, k);
        if (i < 0) {
            i = ~i; // Invert to get index of next larger element
            if (i < values.Length) {
                return values[i];
            }
            return -1;
        }
        return values[i];
    }

    /// <summary>
    /// Determines if the specified <see cref="DateTime"/> falls within the schedule.
    /// </summary>
    /// <param name="time">The date and time to check.</param>
    /// <returns><c>true</c> if the specified date and time matches the schedule; otherwise, <c>false</c>.</returns>
    public bool IsScheduledTime(DateTime time) {
        
        // Determine time units are matching/valid
        bool isMatchingSecond = this.seconds.ValuesSet.Contains(time.Second);
        bool isMatchingMinute = this.minutes.ValuesSet.Contains(time.Minute);
        bool isMatchingHour = this.hours.ValuesSet.Contains(time.Hour);
        
        // Determine day value is matching/valid
        bool isMatchingDayOfWeek = this.dayOfWeek.ValuesSet.Contains(MapDayOfWeek(time.DayOfWeek));
        bool isMatchingDayOfMonth = this.dayOfMonth.ValuesSet.Contains(time.Day);
        bool isMatchingDay = isMatchingDayOfWeek || isMatchingDayOfMonth;

        // Determine if matching month
        bool isMatchingMonth = this.month.ValuesSet.Contains(time.Month);
                
        return isMatchingSecond && isMatchingMinute && isMatchingHour && isMatchingDay && isMatchingMonth;

    }

    private static int MapDayOfWeek(DayOfWeek day) => day switch {
        DayOfWeek.Monday => 0,
        DayOfWeek.Tuesday => 1,
        DayOfWeek.Wednesday => 2,
        DayOfWeek.Thursday => 3,
        DayOfWeek.Friday => 4,
        DayOfWeek.Saturday => 5,
        DayOfWeek.Sunday => 6,
        _ => throw new ArgumentOutOfRangeException(nameof(day))
    };

    /// <summary>
    /// Determines whether an event should have executed between two specified date and times.
    /// </summary>
    /// <param name="from">The start of the date and time range.</param>
    /// <param name="to">The end of the date and time range.</param>
    /// <param name="when">Outputs the exact date and time when the event should have been executed within the given range if any.</param>
    /// <returns><c>true</c> if an event should have executed within the specified range; otherwise, <c>false</c>.</returns>
    public bool WasScheduledBetween(DateTime from, DateTime to, [NotNullWhen(true)] out DateTime? when) {
        DateTime t = GetNextInternal(from, to);
        if (t == to) {
            when = null;
            return false;
        }
        when = t;
        return true;
    }

    /// <summary>
    /// Get a schedule that executes every nth second
    /// </summary>
    /// <param name="seconds">The amount of seconds to pass between a scheduled event.</param>
    /// <returns>A <see cref="Schedule"/> that should run every <paramref name="seconds"/>.</returns>
    public static Schedule EveryNthSecond(int seconds) {
        ArgumentOutOfRangeException.ThrowIfNegative(seconds);
        Field everyNthSecond = new Field(Enumerable.Range(0, 59).Where(x => x % seconds == 0).ToArray());
        return new Schedule(everyNthSecond, EveryMinute, EveryHour, EveryDayOfMonth, EveryDayOfWeek, EveryMonth);
    }

}
