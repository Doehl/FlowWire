using FlowWire.Core;
using System.Buffers.Binary;

namespace FlowWire.Engine.IO;

/// <summary>
/// A flyweight view over a serialized history event.
/// Parsing is lazy and zero-allocation.
/// Layout: [EventId (8b)] [EventType (1b)] [PayloadLength (4b)] [Payload (Var)]
/// </summary>
public readonly ref struct EventView
{
    private readonly ReadOnlySpan<byte> _data;

    public EventView(ReadOnlySpan<byte> data)
    {
        _data = data;
    }

    public long EventId => BinaryPrimitives.ReadInt64LittleEndian(_data.Slice(0, 8));

    public EventType Type => (EventType)_data[8];

    public int PayloadLength => BinaryPrimitives.ReadInt32LittleEndian(_data.Slice(9, 4));

    public ReadOnlySpan<byte> Payload => _data.Slice(13, PayloadLength);

    public int TotalSize => 13 + PayloadLength;
}
