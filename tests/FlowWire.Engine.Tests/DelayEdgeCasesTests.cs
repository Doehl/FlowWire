using FlowWire.Core;
using FlowWire.Engine.Runtime;

namespace FlowWire.Engine.Tests;

public class DelayEdgeCasesTests
{
    [Fact]
    public async Task DelayAsync_Zero_ShouldScheduleTimer()
    {
        // Arrange
        var context = new WorkflowContextImpl();
        context.Initialize(ReadOnlyMemory<byte>.Empty, 0, DateTime.UtcNow, CancellationToken.None);

        // Act
        var task = context.DelayAsync(TimeSpan.Zero, CancellationToken.None);

        // Assert
        Assert.False(task.IsCompleted); // Suspend
        Assert.Equal(1, context.Commands.Count);
        Assert.Equal(EventType.TimerStarted, context.Commands.GetCommands()[0].Type);
    }

    /// <summary>
    /// Tests the behavior of DelayAsync when called with a sequence of delays.
    /// 1. History: [TimerFired(ID=1)]
    /// 2. Code: await Delay(1); await Delay(2);
    /// Expect: First completes, Second suspends + schedules.
    /// </summary>
    [Fact]
    public async Task DelayAsync_Sequence_ReplayThenNew()
    {

        var context = new WorkflowContextImpl();
        var historyBuffer = new HistoryBuilder()
            .Add(1, EventType.TimerFired)
            .Build();

        context.Initialize(historyBuffer, 0, DateTime.UtcNow, CancellationToken.None);

        await context.DelayAsync(TimeSpan.FromSeconds(1), CancellationToken.None);

        Assert.Equal(0, context.Commands.Count);

        var task2 = context.DelayAsync(TimeSpan.FromSeconds(2), CancellationToken.None);

        Assert.False(task2.IsCompleted);
        Assert.Equal(1, context.Commands.Count);
        Assert.Equal(EventType.TimerStarted, context.Commands.GetCommands()[0].Type);
    }

    /// <summary>
    /// Tests the behavior of DelayAsync when called with multiple replays.
    /// 1. History: [TimerFired(ID=1)] [TimerFired(ID=2)]
    /// 2. Code: await Delay(1); await Delay(1);
    /// Expect: Both complete without scheduling new timers.
    /// </summary>
    [Fact]
    public async Task DelayAsync_MultipleReplays()
    {
        var context = new WorkflowContextImpl();
        var historyBuffer = new HistoryBuilder()
            .Add(1, EventType.TimerFired)
            .Add(2, EventType.TimerFired)
            .Build();

        context.Initialize(historyBuffer, 0, DateTime.UtcNow, CancellationToken.None);

        await context.DelayAsync(TimeSpan.FromSeconds(1), CancellationToken.None);
        await context.DelayAsync(TimeSpan.FromSeconds(1), CancellationToken.None);

        Assert.Equal(0, context.Commands.Count);
    }
}
