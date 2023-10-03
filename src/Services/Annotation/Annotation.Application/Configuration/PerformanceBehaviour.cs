namespace PreciPoint.Ims.Services.Annotation.Application.Configuration;

public class PerformanceBehaviour
{
    /// <summary>
    /// define the upper value that triggers the long running requests to be notified
    /// </summary>
    public int LongRunningTriggerMilliseconds { get; set; }
}