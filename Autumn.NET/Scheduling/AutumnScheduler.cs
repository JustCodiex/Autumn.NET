using System.Reflection;

namespace Autumn.Scheduling;

public sealed class AutumnScheduler {

    private record ScheduledMethod(ISchedule Schedule, object Target, MethodInfo MethodInfo) {
        public DateTime NextInvocation { get; set; }
        public async void Invoke() 
            => await Task.Run(() => MethodInfo.Invoke(Target, Array.Empty<object>()));
    }

    private List<ScheduledMethod> scheduledMethods;
    private CancellationTokenSource? cancellationToken;

    public AutumnScheduler() {
        this.scheduledMethods = new List<ScheduledMethod>();
    }

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

    public void Shutdown() {
        cancellationToken?.Cancel();
        cancellationToken = null;
    }

    public void Schedule(object methodTarget, MethodInfo methodInfo, ISchedulerAttribute scheduledAttribute) {
        ISchedule schedule = scheduledAttribute.GetSchedule();
        ScheduledMethod scheduledMethod = new ScheduledMethod(schedule, methodTarget, methodInfo) { 
            NextInvocation = schedule.GetNext(DateTime.Now)
        };
        scheduledMethods.Add(scheduledMethod);
    }

}
