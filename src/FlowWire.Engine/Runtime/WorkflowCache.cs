using System.Collections.Concurrent;

namespace FlowWire.Engine.Runtime;

internal class WorkflowCache
{
    private readonly ConcurrentDictionary<string, WorkflowContextImpl> _cache = new();

    public bool TryGet(string runId, out WorkflowContextImpl? context)
    {
        return _cache.TryGetValue(runId, out context);
    }

    public void Add(string runId, WorkflowContextImpl context)
    {
        _cache[runId] = context;
    }

    public void Remove(string runId)
    {
        _cache.TryRemove(runId, out _);
    }
}
