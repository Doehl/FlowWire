using FlowWire.Engine.Runtime;
using Microsoft.Extensions.ObjectPool;

namespace FlowWire.Engine.Tests;

public class PoolingTests
{
    [Fact]
    public void Context_ShouldBeResettable_AndReused()
    {
        var provider = new DefaultObjectPoolProvider();
        var pool = provider.Create(new WorkflowContextPolicy());

        var ctx1 = pool.Get();
        ctx1.Initialize(ReadOnlyMemory<byte>.Empty, 0, DateTime.UtcNow, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, ctx1.NewGuid());

        pool.Return(ctx1);

        var ctx2 = pool.Get();

        Assert.Same(ctx1, ctx2);

        Assert.Throws<Exceptions.InvalidWorkflowContextException>(() => _ = ctx2.Now);

        ctx2.Initialize(ReadOnlyMemory<byte>.Empty, 0, DateTime.UtcNow, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, ctx2.NewGuid());
    }
}
