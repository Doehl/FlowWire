using BenchmarkDotNet.Attributes;
using FlowWire.Engine.Internals;
using FlowWire.Engine.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace FlowWire.Benchmarks;

[MemoryDiagnoser]
public class EngineBenchmarks
{
    private WorkflowExecutor? _pooledExecutor;
    private AllocatingWorkflowExecutor? _allocatingExecutor;
    private IServiceProvider? _serviceProvider;
    private byte[]? _history;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();
        _serviceProvider = services.BuildServiceProvider();
        _pooledExecutor = new WorkflowExecutor(_serviceProvider);
        _allocatingExecutor = new AllocatingWorkflowExecutor(_serviceProvider);
        _history = [];
    }

    [Benchmark(Baseline = true)]
    public async Task Pooled()
    {
        await _pooledExecutor!.ExecuteBatchAsync<BenchWorkflow, int, int>(
            _history!,
            100,
            (w, i) => w.RunAsync(i));
    }

    [Benchmark]
    public async Task Allocating()
    {
        await _allocatingExecutor!.ExecuteBatchAsync<BenchWorkflow, int, int>(
            _history!,
            100,
            (w, i) => w.RunAsync(i));
    }
}

public class BenchWorkflow
{
    private readonly IWorkflowContext _ctx;
    public BenchWorkflow(IWorkflowContext ctx) => _ctx = ctx;

    public async ValueTask<int> RunAsync(int input)
    {
        await _ctx.CallActivityAsync<int, int>("activity-1", input);
        return input;
    }
}

public class AllocatingWorkflowExecutor
{
    private readonly IServiceProvider _serviceProvider;

    public AllocatingWorkflowExecutor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<ExecutionResult> ExecuteBatchAsync<TWorkflow, TInput, TResult>(
        ReadOnlyMemory<byte> history,
        TInput input,
        Func<TWorkflow, TInput, ValueTask<TResult>> runDelegate,
        CancellationToken cancellationToken = default) where TWorkflow : class
    {
        var context = new WorkflowContextImpl();
        context.Initialize(history, DateTime.UtcNow.Ticks, DateTime.UtcNow, cancellationToken);

        try
        {
            var workflow = ActivatorUtilities.CreateInstance<TWorkflow>(_serviceProvider, (IWorkflowContext)context);
            var task = runDelegate(workflow, input);

            var resultQueue = new CommandQueue(context.Commands.Count);
            resultQueue.EnqueueRange(context.Commands.GetCommands());

            return task.IsCompleted
                ? new ExecutionResult(ExecutionStatus.Completed, resultQueue, await task)
                : new ExecutionResult(ExecutionStatus.Suspended, resultQueue, null);
        }
        catch (Exception ex)
        {
            return new ExecutionResult(ExecutionStatus.Failed, new(), ex);
        }
    }
}
