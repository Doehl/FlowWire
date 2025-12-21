namespace FlowWire;

public record ActivityOptions
{
    public string? TaskQueue { get; init; }
    
    // Timeouts
    public TimeSpan? ScheduleToCloseTimeout { get; init; }
    public TimeSpan? ScheduleToStartTimeout { get; init; }
    public TimeSpan? StartToCloseTimeout { get; init; }
    
    // Retry Policy? Usually defined on the Activity Attribute, but can be overridden here?
    // Keeping it simple for now.
}
