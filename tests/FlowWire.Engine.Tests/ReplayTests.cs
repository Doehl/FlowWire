using FlowWire.Core;
using FlowWire.Engine.Runtime;

namespace FlowWire.Engine.Tests;

public class ReplayTests
{
    /// <summary>
    /// Tests that the DelayAsync method completes immediately if the delay event is in the history.
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task DelayAsync_ShouldCompleteImmediately_IfInHistory()
    {
        var context = new WorkflowContextImpl();

        var historyBuffer = new HistoryBuilder()
            .Add(1, EventType.TimerFired)
            .Build();

        context.Initialize(historyBuffer, 0, DateTime.UtcNow, CancellationToken.None);

        await context.DelayAsync(TimeSpan.FromSeconds(5), CancellationToken.None);

        Assert.Equal(0, context.Commands.Count);
    }

    [Fact]
    public async Task DelayAsync_ShouldScheduleCommand_IfNew()
    {
        var context = new WorkflowContextImpl();

        context.Initialize(ReadOnlyMemory<byte>.Empty, 0, DateTime.UtcNow, CancellationToken.None);

        var task = context.DelayAsync(TimeSpan.FromSeconds(5), CancellationToken.None);

        Assert.False(task.IsCompleted);

        Assert.Equal(1, context.Commands.Count);
        Assert.Equal(EventType.TimerStarted, context.Commands.GetCommands()[0].Type);
    }
}
