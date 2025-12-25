using Microsoft.Extensions.ObjectPool;

namespace FlowWire.Engine.Runtime;

internal class WorkflowContextPolicy : IPooledObjectPolicy<WorkflowContextImpl>
{
    public WorkflowContextImpl Create() => new();

    public bool Return(WorkflowContextImpl obj)
    {
        return obj.TryReset();
    }
}
