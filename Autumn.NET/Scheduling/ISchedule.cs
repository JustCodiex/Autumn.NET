using System.Diagnostics.CodeAnalysis;

namespace Autumn.Scheduling;

/// <summary>
/// Represents a scheduling system that determines the timing of events.
/// </summary>
public interface ISchedule {

    /// <summary>
    /// Retrieves the next scheduled <see cref="DateTime"/> after the provided date and time.
    /// </summary>
    /// <param name="from">The date and time to start the search from.</param>
    /// <returns>The next scheduled date and time after the provided one.</returns>
    DateTime GetNext(DateTime from);

    /// <summary>
    /// Determines whether an event was scheduled between two specified date and times.
    /// </summary>
    /// <param name="from">The start of the date and time range.</param>
    /// <param name="to">The end of the date and time range.</param>
    /// <param name="when">Outputs the exact date and time when the event was scheduled within the given range if any.</param>
    /// <returns><c>true</c> if an event was scheduled within the specified range; otherwise, <c>false</c>.</returns>
    bool WasScheduledBetween(DateTime from, DateTime to, [NotNullWhen(true)] out DateTime? when);

}
