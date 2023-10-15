namespace Autumn.Scheduling;

/// <summary>
/// Represents an interface for a scheduler attribute that can be registered with the <see cref="AutumnScheduler"/>.
/// </summary>
public interface ISchedulerAttribute {

    /// <summary>
    /// Retrieves the <see cref="ISchedule"/> represented by this attribute.
    /// </summary>
    /// <returns>The <see cref="ISchedule"/>.</returns>
    ISchedule GetSchedule();

}
