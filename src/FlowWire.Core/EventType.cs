namespace FlowWire.Core;

/// <summary>
/// Represents the type of a history event in the FlowWire protocol.
/// </summary>
public enum EventType : byte
{
    Unspecified = 0,
    
    // Workflow Lifecycle
    WorkflowExecutionStarted = 1,
    WorkflowExecutionCompleted = 2,
    WorkflowExecutionFailed = 3,
    WorkflowExecutionTerminated = 4,

    // Task Scheduling
    ActivityScheduled = 10,
    ActivityCompleted = 11,
    ActivityFailed = 12,
    ActivityTimedOut = 13,

    // Timers
    TimerStarted = 20,
    TimerFired = 21,

    // External Events
    SignalReceived = 30,
    WorkflowUpdateAccepted = 31,
    WorkflowUpdateCompleted = 32,
    
    // Markers
    SideEffectRecorded = 40,
    VersionMarker = 41
}
