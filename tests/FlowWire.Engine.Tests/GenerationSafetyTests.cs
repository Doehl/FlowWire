using FlowWire.Engine.Exceptions;
using FlowWire.Engine.Runtime;

namespace FlowWire.Engine.Tests;

public class GenerationSafetyTests
{
    [Fact]
    public void Context_ShouldThrow_WhenAccessedAndStale()
    {
        var context = new WorkflowContextImpl();
        context.Initialize(ReadOnlyMemory<byte>.Empty, 0, DateTime.UtcNow, CancellationToken.None);

        var guid = context.NewGuid();
        Assert.NotEqual(Guid.Empty, guid);

        context.TryReset();

        Assert.Throws<InvalidWorkflowContextException>(() => context.NewGuid());
        Assert.Throws<InvalidWorkflowContextException>(() => _ = context.Now);
    }

    [Fact]
    public void Context_ShouldRecover_WhenReinitialized()
    {
        var context = new WorkflowContextImpl();

        context.Initialize(ReadOnlyMemory<byte>.Empty, 0, DateTime.UtcNow, CancellationToken.None);
        context.TryReset();

        context.Initialize(ReadOnlyMemory<byte>.Empty, 0, DateTime.UtcNow, CancellationToken.None);

        var guid = context.NewGuid();
        Assert.NotEqual(Guid.Empty, guid);
    }
}
