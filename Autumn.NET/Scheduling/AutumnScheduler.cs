using System.Reflection;

namespace Autumn.Scheduling;

/// <summary>
/// Provides a scheduling utility to execute methods at specified times based on provided schedules.
/// This class cannot be inherited.
/// </summary>
public sealed class AutumnScheduler {

    private record ScheduledMethod(ISchedule Schedule, object Target, MethodInfo MethodInfo) {
        public DateTime NextInvocation { get; set; }
        public async void Invoke() 
            => await Task.Run(() => MethodInfo.Invoke(Target, Array.Empty<object>()));
    }

    private readonly List<ScheduledMethod> scheduledMethods;
    private CancellationTokenSource? cancellationToken;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutumnScheduler"/> class.
    /// </summary>
    public AutumnScheduler() {
        this.scheduledMethods = new List<ScheduledMethod>();
    }

    /// <summary>
    /// Starts the scheduler loop, enabling scheduled methods to be invoked.
    /// If the scheduler is already running, this method does nothing.
    /// </summary>
    public void Start() {
        if (cancellationToken is not null) {
            return; // Already running...
        }
        cancellationToken = new CancellationTokenSource();
        RunLoop(cancellationToken.Token);
    }

    private async void RunLoop(CancellationToken token) {
        while (!token.IsCancellationRequested) {
            DateTime now = DateTime.Now;
            foreach (var method in scheduledMethods) {
                if (now >= method.NextInvocation) {
                    method.NextInvocation = method.Schedule.GetNext(now);
                    method.Invoke();
                }
            }
            await Task.Delay(100, token);
        }
    }

    /// <summary>
    /// Shuts down the scheduler, ensuring that no new methods will be invoked.
    /// </summary>
    public void Shutdown() {
        cancellationToken?.Cancel();
        cancellationToken = null;
    }

    /// <summary>
    /// Schedules a method to be invoked by the scheduler.
    /// </summary>
    /// <param name="methodTarget">The target object on which the method will be invoked.</param>
    /// <param name="methodInfo">The information of the method to be invoked.</param>
    /// <param name="scheduledAttribute">The scheduling details, defining when the method should be invoked.</param>
    public void Schedule(object methodTarget, MethodInfo methodInfo, ISchedulerAttribute scheduledAttribute) {
        ISchedule schedule = scheduledAttribute.GetSchedule();
        ScheduledMethod scheduledMethod = new ScheduledMethod(schedule, methodTarget, methodInfo) { 
            NextInvocation = schedule.GetNext(DateTime.Now)
        };
        scheduledMethods.Add(scheduledMethod);
    }

}
