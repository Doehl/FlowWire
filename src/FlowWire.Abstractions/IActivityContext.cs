namespace FlowWire;

public interface IActivityContext
{
    /// <summary>
    /// Gets the cancellation token. Triggered if the workflow cancels the activity or the worker shuts down.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Records a heartbeat to the orchestrator.
    /// </summary>
    /// <param name="details">Optional progress details.</param>
    Task HeartbeatAsync(object? details = null);
}
