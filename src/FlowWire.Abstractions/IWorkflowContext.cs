namespace FlowWire;

public interface IWorkflowContext
{
    /// <summary>
    /// Gets the current deterministic time (UTC).
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Generates a deterministic GUID.
    /// </summary>
    Guid NewGuid();

    /// <summary>
    /// Gets a deterministic Random instance seeded by the framework.
    /// </summary>
    Random Random { get; }

    /// <summary>
    /// Gets the cancellation token for the workflow execution.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Pauses execution for the specified duration using the workflow timer.
    /// </summary>
    Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes an Activity by name with inputs.
    /// </summary>
    Task<TResult> CallActivityAsync<TInput, TResult>(string name, TInput input, ActivityOptions? options = null);

    /// <summary>
    /// Invokes an Activity by name with inputs (no result).
    /// </summary>
    Task CallActivityAsync<TInput>(string name, TInput input, ActivityOptions? options = null);
}
