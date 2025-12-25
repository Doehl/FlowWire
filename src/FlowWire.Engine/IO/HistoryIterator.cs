namespace FlowWire.Engine.IO;

/// <summary>
/// Iterates over a contiguous block of memory containing history events.
/// </summary>
public ref struct HistoryIterator
{
    private ReadOnlySpan<byte> _remaining;
    private EventView _current;

    public HistoryIterator(ReadOnlySpan<byte> historyLog)
    {
        _remaining = historyLog;
        _current = default;
    }

    public readonly EventView Current => _current;

    /// <summary>
    /// Advances to the next event.
    /// </summary>
    /// <returns>True if an event was found; false if end of stream.</returns>
    public bool MoveNext()
    {
        if (_remaining.Length < 13)
        {
            return false;
        }

        // Parse header to know length
        _current = new EventView(_remaining);

        int size = _current.TotalSize;
        if (_remaining.Length < size)
        {
            return false;
        }

        _remaining = _remaining.Slice(size);
        return true;
    }
}
