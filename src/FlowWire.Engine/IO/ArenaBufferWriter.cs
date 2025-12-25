using FlowWire.Engine.Runtime;
using System.Buffers;

namespace FlowWire.Engine.IO;

/// <summary>
/// Adapts the Arena to IBufferWriter for zero-allocation JSON serialization.
/// </summary>
public sealed class ArenaBufferWriter(Arena arena) : IBufferWriter<byte>
{
    public void Advance(int count)
    {
        arena.Advance(count);
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        return arena.GetMemory(sizeHint);
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        return arena.GetSpan(sizeHint);
    }
}
