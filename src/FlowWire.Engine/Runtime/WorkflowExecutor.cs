using FlowWire.Engine.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;

namespace FlowWire.Engine.Runtime;

public class WorkflowExecutor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ObjectPool<WorkflowContextImpl> _contextPool;

    public WorkflowExecutor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        // Setup Pool
        var provider = new DefaultObjectPoolProvider();
        _contextPool = provider.Create(new WorkflowContextPolicy());
    }

    public async Task<ExecutionResult> ExecuteBatchAsync<TWorkflow, TInput, TResult>(
        ReadOnlyMemory<byte> history,
        TInput input,
        Func<TWorkflow, TInput, ValueTask<TResult>> runDelegate,
        CancellationToken cancellationToken = default) where TWorkflow : class
    {
        var context = _contextPool.Get();
        context.Initialize(history, DateTime.UtcNow.Ticks, DateTime.UtcNow, cancellationToken); // TODO: Seed must come from history

        try
        {
            var workflow = ActivatorUtilities.CreateInstance<TWorkflow>(_serviceProvider, context);

            var workflowTask = runDelegate(workflow, input);

            var resultQueue = new CommandQueue(context.Commands.Count);
            resultQueue.EnqueueRange(context.Commands.GetCommands());

            if (!workflowTask.IsCompleted)
            {
                return new ExecutionResult(ExecutionStatus.Suspended, resultQueue, null);
            }

            var result = await workflowTask;
            return new ExecutionResult(ExecutionStatus.Completed, resultQueue, result);
        }
        catch (Exception ex)
        {
            return new ExecutionResult(ExecutionStatus.Failed, new CommandQueue(), ex);
        }
        finally
        {
            _contextPool.Return(context);
        }
    }
}
