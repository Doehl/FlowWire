using FlowWire.Core;
using System.Buffers.Binary;

namespace FlowWire.Engine.Tests;

public class HistoryBuilder
{
    private readonly MemoryStream _stream = new();

    public HistoryBuilder Add(long eventId, EventType type, byte[]? payload = null)
    {
        Span<byte> header = stackalloc byte[13];
        BinaryPrimitives.WriteInt64LittleEndian(header[..8], eventId);
        header[8] = (byte)type;
        BinaryPrimitives.WriteInt32LittleEndian(header.Slice(9, 4), payload?.Length ?? 0);

        _stream.Write(header);
        if (payload != null && payload.Length > 0)
        {
            _stream.Write(payload);
        }

        return this;
    }

    public byte[] Build()
    {
        return _stream.ToArray();
    }
}
